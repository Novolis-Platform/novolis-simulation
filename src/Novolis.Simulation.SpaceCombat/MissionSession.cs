using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

public sealed class MissionSession
{
    private readonly Random _rng;
    private readonly LaserBolt[] _playerBolts;
    private readonly LaserBolt[] _enemyBolts;
    private float _protectTimer;
    private float _playerFireCooldown;
    private int _kills;
    private bool _transferArmed;
    private IFlightController _crewPilotAi;
    private IFlightController _crewGunnerAi;
    private IFlightController _freighterPilotAi;

    public MissionSession(MissionDescriptor descriptor, int seed = 42)
    {
        Descriptor = descriptor;
        _rng = new Random(seed);
        Freighter = new CraftState { Profile = descriptor.FreighterProfile, PlayerControlled = true, Speed = 12f };
        Freighter.ResetVitals();
        Fighter = new CraftState { Profile = descriptor.FighterProfile, PlayerControlled = false, Speed = 22f };
        Fighter.ResetVitals();
        Fighter.Position = Freighter.Position + Freighter.Forward * -8f;

        Hostiles = Enumerable.Range(0, descriptor.MaxHostilesAlive)
            .Select(_ => new CraftState { Profile = descriptor.HostileProfile })
            .ToArray();
        foreach (var h in Hostiles)
            h.Active = false;

        _playerBolts = Enumerable.Range(0, 48).Select(_ => new LaserBolt()).ToArray();
        _enemyBolts = Enumerable.Range(0, 32).Select(_ => new LaserBolt { FromPlayer = false }).ToArray();
        _protectTimer = descriptor.ProtectSeconds;
        Phase = MissionPhase.Freighter;
        Player = Freighter;
        _crewPilotAi = new HeuristicPilotAi();
        _crewGunnerAi = new HeuristicGunnerAi();
        _freighterPilotAi = new HeuristicPilotAi(engageDistance: 70f, turnGain: 0.028f);
        CrewStation = CrewStation.Dual;
    }

    public MissionDescriptor Descriptor { get; }
    public MissionPhase Phase { get; private set; }
    public CraftState Freighter { get; }
    public CraftState Fighter { get; }
    public CraftState Player { get; private set; }
    public CraftState[] Hostiles { get; }
    public IReadOnlyList<LaserBolt> PlayerBolts => _playerBolts;
    public IReadOnlyList<LaserBolt> EnemyBolts => _enemyBolts;
    public int Kills => _kills;
    public float ProtectRemaining => Math.Max(0, _protectTimer);
    public bool CanTransfer => Phase == MissionPhase.Freighter && _transferArmed;
    public int ActiveHostiles => Hostiles.Count(h => h.Active);

    /// <summary>
    /// Human crew role. <see cref="CrewStation.Pilot"/> enables AI gunner;
    /// <see cref="CrewStation.Gunner"/> enables AI pilot.
    /// </summary>
    public CrewStation CrewStation { get; set; }

    /// <summary>Optional override for crew AI (e.g. neural-imitation controllers).</summary>
    public void SetCrewControllers(
        IFlightController? pilot = null,
        IFlightController? gunner = null,
        IFlightController? freighterPilot = null)
    {
        if (pilot is not null)
            _crewPilotAi = pilot;
        if (gunner is not null)
            _crewGunnerAi = gunner;
        if (freighterPilot is not null)
            _freighterPilotAi = freighterPilot;
    }

    public void Begin()
    {
        SpawnWave(Math.Min(3, Descriptor.HostileCount));
        _transferArmed = false;
    }

    public void Tick(in FlightIntent playerIntent, float dt)
    {
        if (Phase is MissionPhase.Complete or MissionPhase.Failed)
            return;

        if (Phase == MissionPhase.Freighter && playerIntent.Transfer && CanTransfer)
            TransferToFighter();

        var intent = ComposePlayerIntent(playerIntent, dt);

        var controlled = Player;
        if (controlled.PlayerControlled)
            ArcadeFlight.Apply(controlled, intent, dt);

        if (!Freighter.PlayerControlled && Freighter.Active)
            TickFreighterAi(dt);

        UpdateHostiles(dt);
        UpdateCombat(intent, dt);
        EvaluateObjectives(dt);
    }

    private FlightIntent ComposePlayerIntent(in FlightIntent playerIntent, float dt)
    {
        if (!Player.PlayerControlled || CrewStation == CrewStation.Dual)
            return playerIntent;

        var observation = CraftObservation.FromSession(this, Player, dt);
        var aiPilot = _crewPilotAi.Tick(observation);
        var aiGunner = _crewGunnerAi.Tick(observation);
        return CrewIntentComposer.Compose(CrewStation, playerIntent, aiPilot, aiGunner);
    }

    private void TickFreighterAi(float dt)
    {
        var observation = CraftObservation.FromSession(this, Freighter, dt);
        var intent = _freighterPilotAi.Tick(observation);
        ArcadeFlight.Apply(Freighter, intent, dt);
    }

    public CraftState? LockTarget =>
        Targeting.FindLockTarget(Hostiles, Player.Position, Player.Forward);

    private void TransferToFighter()
    {
        Freighter.PlayerControlled = false;
        Fighter.PlayerControlled = true;
        Fighter.Position = Freighter.Position + Freighter.Forward * 6f + new Vector3(0, 1.5f, 0);
        Fighter.Yaw = Freighter.Yaw;
        Fighter.Pitch = Freighter.Pitch;
        Fighter.Speed = Math.Max(Fighter.Profile.MinSpeed + 4f, Freighter.Speed + 8f);
        Fighter.ResetVitals();
        Player = Fighter;
        Phase = MissionPhase.Fighter;
        SpawnWave(Descriptor.HostileCount - ActiveHostiles);
    }

    private void UpdateHostiles(float dt)
    {
        var playerPos = Player.Position;
        var active = ActiveHostiles;
        foreach (var h in Hostiles)
        {
            HostileAi.Update(h, playerPos, Hostiles, dt);
            if (!HostileAi.TryFire(h, playerPos, active))
                continue;
            HostileAi.GetBoltVelocity(h, playerPos, out var origin, out var velocity);
            BoltPools.TrySpawn(_enemyBolts, origin, velocity, 2f, fromPlayer: false, damage: 0.1f);
        }
    }

    private void UpdateCombat(in FlightIntent intent, float dt)
    {
        _playerFireCooldown = Math.Max(0, _playerFireCooldown - dt);
        if (intent.Fire && _playerFireCooldown <= 0 && Player.PlayerControlled)
        {
            _playerFireCooldown = Player.Profile.Role == CraftRole.Freighter ? 0.35f : 0.14f;
            var origin = Player.Position + Player.Forward * 2f;
            var speed = Player.Profile.Role == CraftRole.Freighter ? 70f : 100f;
            BoltPools.TrySpawn(_playerBolts, origin, Player.Forward * speed, 2.2f, fromPlayer: true,
                damage: Player.Profile.Role == CraftRole.Freighter ? 0.35f : 0.55f);
        }

        BoltPools.Update(_playerBolts, dt, Player.Position, 200f);
        BoltPools.Update(_enemyBolts, dt, Player.Position, 90f);

        foreach (var bolt in _playerBolts)
        {
            if (!bolt.Active)
                continue;
            var prev = bolt.Position - bolt.Velocity * Math.Min(dt, 0.02f);
            foreach (var enemy in Hostiles)
            {
                if (!enemy.Active)
                    continue;
                if (!CombatHits.SegmentHitsSphere(prev, bolt.Position, enemy.Position, enemy.Profile.HitRadius))
                    continue;

                bolt.Active = false;
                enemy.Hull -= bolt.Damage;
                if (enemy.Hull > 0)
                    continue;
                enemy.Active = false;
                _kills++;
                break;
            }
        }

        foreach (var bolt in _enemyBolts)
        {
            if (!bolt.Active)
                continue;
            var target = Phase == MissionPhase.Fighter ? PickHostileBoltTarget() : Player;
            if (!CombatHits.SegmentHitsSphere(
                    bolt.Position - bolt.Velocity * Math.Min(dt, 0.02f),
                    bolt.Position,
                    target.Position,
                    target.Profile.HitRadius))
                continue;

            bolt.Active = false;
            ApplyDamage(target, bolt.Damage);
        }
    }

    private CraftState PickHostileBoltTarget()
    {
        // Prefer shooting the freighter objective when in fighter phase
        if (Freighter.Active && _rng.NextDouble() < 0.55)
            return Freighter;
        return Player;
    }

    private void ApplyDamage(CraftState craft, float damage)
    {
        var absorb = Math.Min(craft.Shield, damage);
        craft.Shield -= absorb;
        var rest = damage - absorb;
        if (rest > 0)
            craft.Hull -= rest;
        if (craft.Hull <= 0)
        {
            craft.Active = false;
            if (craft.PlayerControlled || ReferenceEquals(craft, Freighter))
                Phase = MissionPhase.Failed;
        }
    }

    private void EvaluateObjectives(float dt)
    {
        if (Phase == MissionPhase.Freighter)
        {
            _protectTimer -= dt;
            if (_protectTimer <= 0 || _kills >= 2)
                _transferArmed = true;
        }

        if (Phase == MissionPhase.Fighter)
        {
            if (!Freighter.Active)
            {
                Phase = MissionPhase.Failed;
                return;
            }

            if (_kills >= Descriptor.DestroyRequired)
                Phase = MissionPhase.Complete;
        }
    }

    private void SpawnWave(int count)
    {
        var spawned = 0;
        foreach (var h in Hostiles)
        {
            if (h.Active || spawned >= count)
                continue;
            HostileAi.SpawnNear(h, Player.Position, Player.Forward, _rng);
            spawned++;
        }
    }
}
