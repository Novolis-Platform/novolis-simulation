using System.Numerics;
using Novolis.Simulation.SpaceCombat;
using Novolis.Simulation.View;

namespace Novolis.Simulation.Unit.SpaceCombat;

public class SpaceCombatTests
{
    [Test]
    public async Task ArcadeFlight_AdvancesPosition()
    {
        var craft = new CraftState { Profile = CraftProfile.FighterDefault, Speed = 20f };
        craft.ResetVitals();
        var intent = new FlightIntent { ThrottleUp = 1f };
        ArcadeFlight.Apply(craft, intent, 0.5f);
        await Assert.That(craft.Position.Length()).IsGreaterThan(0.1f);
    }

    [Test]
    public async Task SegmentHitsSphere_DetectsHit()
    {
        var hit = CombatHits.SegmentHitsSphere(
            new Vector3(0, 0, 0),
            new Vector3(0, 0, 10),
            new Vector3(0, 0, 5),
            1f);
        await Assert.That(hit).IsTrue();
    }

    [Test]
    public async Task MissionSession_TransferAndComplete()
    {
        var session = new MissionSession(new MissionDescriptor
        {
            Id = "test",
            FreighterProfile = CraftProfile.FreighterDefault,
            FighterProfile = CraftProfile.FighterDefault,
            HostileProfile = CraftProfile.HostileDefault,
            HostileCount = 1,
            ProtectSeconds = 0.01f,
            DestroyRequired = 1,
            MaxHostilesAlive = 4,
        }, seed: 2);
        session.Begin();

        for (var i = 0; i < 40 && !session.CanTransfer; i++)
            session.Tick(default, 0.05f);

        await Assert.That(session.CanTransfer).IsTrue();
        session.Tick(new FlightIntent { Transfer = true }, 0.016f);
        await Assert.That(session.Phase).IsEqualTo(MissionPhase.Fighter);

        var target = session.Hostiles.First(h => h.Active);
        for (var i = 0; i < 80 && session.Phase == MissionPhase.Fighter; i++)
        {
            target.Position = session.Player.Position + session.Player.Forward * 8f;
            session.Tick(new FlightIntent { Fire = true }, 0.05f);
        }

        await Assert.That(session.Phase).IsEqualTo(MissionPhase.Complete);
    }

    [Test]
    public async Task CrewComposer_PilotUsesAiFire()
    {
        var player = new FlightIntent { YawDelta = 0.01f, ThrottleUp = 1f };
        var aiPilot = new FlightIntent { ThrottleUp = 0.2f };
        var aiGunner = new FlightIntent { Fire = true, YawDelta = 0.02f };
        var merged = CrewIntentComposer.Compose(CrewStation.Pilot, player, aiPilot, aiGunner);
        await Assert.That(merged.Fire).IsTrue();
        await Assert.That(merged.ThrottleUp).IsEqualTo(1f);
        await Assert.That(merged.YawDelta).IsGreaterThan(player.YawDelta);
    }

    [Test]
    public async Task CrewComposer_GunnerUsesAiThrottle()
    {
        var player = new FlightIntent { Fire = true, YawDelta = 0.03f };
        var aiPilot = new FlightIntent { ThrottleUp = 1f, RollRight = 0.5f, YawDelta = 0.02f };
        var aiGunner = new FlightIntent { Fire = true };
        var merged = CrewIntentComposer.Compose(CrewStation.Gunner, player, aiPilot, aiGunner);
        await Assert.That(merged.ThrottleUp).IsEqualTo(1f);
        await Assert.That(merged.RollRight).IsEqualTo(0.5f);
        await Assert.That(merged.Fire).IsTrue();
        await Assert.That(merged.YawDelta).IsGreaterThan(0.03f);
    }

    [Test]
    public async Task HeuristicGunner_FiresWhenTargetInCone()
    {
        var self = new CraftState { Profile = CraftProfile.FighterDefault, Speed = 24f };
        self.ResetVitals();
        self.Position = Vector3.Zero;
        self.Yaw = 0f;
        self.Pitch = 0f;
        var gunner = new HeuristicGunnerAi(fireConeDot: 0.85f);
        var intent = gunner.Tick(new CraftObservation
        {
            Self = self,
            TargetPosition = new Vector3(0, 0, 30f),
            TargetVelocity = Vector3.Zero,
            EscortAnchor = Vector3.Zero,
            ActiveThreats = 1,
            Dt = 1f / 60f,
        });
        await Assert.That(intent.Fire).IsTrue();
    }

    [Test]
    public async Task MissionSession_GunnerStation_AdvancesWithoutPlayerThrottle()
    {
        var session = new MissionSession(new MissionDescriptor
        {
            Id = "crew",
            FreighterProfile = CraftProfile.FreighterDefault,
            FighterProfile = CraftProfile.FighterDefault,
            HostileProfile = CraftProfile.HostileDefault,
            HostileCount = 1,
            ProtectSeconds = 30f,
            DestroyRequired = 1,
            MaxHostilesAlive = 4,
        }, seed: 9);
        session.Begin();
        session.CrewStation = CrewStation.Gunner;
        var start = session.Player.Position;
        for (var i = 0; i < 30; i++)
            session.Tick(new FlightIntent { Fire = false }, 0.05f);
        await Assert.That(Vector3.Distance(session.Player.Position, start)).IsGreaterThan(0.5f);
    }

    [Test]
    public async Task MissionSession_AfterTransfer_FreighterKeepsMoving()
    {
        var session = new MissionSession(new MissionDescriptor
        {
            Id = "escort-ai",
            FreighterProfile = CraftProfile.FreighterDefault,
            FighterProfile = CraftProfile.FighterDefault,
            HostileProfile = CraftProfile.HostileDefault,
            HostileCount = 1,
            ProtectSeconds = 0.01f,
            DestroyRequired = 1,
            MaxHostilesAlive = 4,
        }, seed: 3);
        session.Begin();
        for (var i = 0; i < 40 && !session.CanTransfer; i++)
            session.Tick(default, 0.05f);
        session.Tick(new FlightIntent { Transfer = true }, 0.016f);
        var freighterStart = session.Freighter.Position;
        for (var i = 0; i < 40; i++)
            session.Tick(new FlightIntent { ThrottleUp = 1f }, 0.05f);
        await Assert.That(Vector3.Distance(session.Freighter.Position, freighterStart)).IsGreaterThan(1f);
    }

    [Test]
    public async Task CraftCamera_Cockpit_HasForwardTarget()
    {
        var pose = CraftCamera.Cockpit(Vector3.Zero, new Vector3(0, 0, 1), 0f);
        await Assert.That(pose.Target.Z).IsGreaterThan(pose.Position.Z);
    }
}
