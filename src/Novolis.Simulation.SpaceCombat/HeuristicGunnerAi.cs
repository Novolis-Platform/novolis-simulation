using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

/// <summary>Heuristic gunner: aim-assist toward lock + auto-fire in cone.</summary>
public sealed class HeuristicGunnerAi : IFlightController
{
    private readonly float _fireConeDot;
    private readonly float _aimGain;
    private readonly float _maxFireRange;
    private readonly float _minFireRange;

    public HeuristicGunnerAi(
        float fireConeDot = 0.92f,
        float aimGain = 0.03f,
        float maxFireRange = 78f,
        float minFireRange = 10f)
    {
        _fireConeDot = fireConeDot;
        _aimGain = aimGain;
        _maxFireRange = maxFireRange;
        _minFireRange = minFireRange;
    }

    public FlightIntent Tick(in CraftObservation observation)
    {
        var intent = new FlightIntent();
        if (observation.TargetPosition is not { } target)
            return intent;

        var lead = target;
        if (observation.TargetVelocity is { } tv)
        {
            var dist = Vector3.Distance(observation.Self.Position, target);
            lead = target + tv * (dist / 95f);
        }

        CrewIntentComposer.AimError(observation.Self, lead, out var yawErr, out var pitchErr);
        intent.YawDelta = yawErr * _aimGain;
        intent.PitchDelta = pitchErr * _aimGain;

        var to = lead - observation.Self.Position;
        var distSq = to.LengthSquared();
        if (distSq < _minFireRange * _minFireRange || distSq > _maxFireRange * _maxFireRange)
            return intent;

        var dir = Vector3.Normalize(to);
        var dot = Vector3.Dot(observation.Self.Forward, dir);
        intent.Fire = dot >= _fireConeDot;
        return intent;
    }
}
