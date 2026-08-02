using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

/// <summary>Heuristic escort / engage pilot for friendly craft.</summary>
public sealed class HeuristicPilotAi : IFlightController
{
    private readonly float _engageDistance;
    private readonly float _turnGain;

    public HeuristicPilotAi(float engageDistance = 48f, float turnGain = 0.045f)
    {
        _engageDistance = engageDistance;
        _turnGain = turnGain;
    }

    public FlightIntent Tick(in CraftObservation observation)
    {
        var self = observation.Self;
        Vector3 aimPoint;
        float throttleBias;

        if (observation.TargetPosition is { } threat)
        {
            var toThreat = threat - self.Position;
            var dist = toThreat.Length();
            // Lead the target slightly when we have velocity.
            if (observation.TargetVelocity is { } tv && dist > 1f)
                aimPoint = threat + tv * (dist / 90f);
            else
                aimPoint = threat;

            throttleBias = dist > _engageDistance ? 1f : dist < _engageDistance * 0.45f ? 0f : 0.55f;
        }
        else
        {
            // Hold escort path: push past freighter anchor along current forward.
            aimPoint = observation.EscortAnchor + self.Forward * 40f + new Vector3(0, 2f, 0);
            throttleBias = 0.7f;
        }

        CrewIntentComposer.AimError(self, aimPoint, out var yawErr, out var pitchErr);

        var intent = new FlightIntent
        {
            YawDelta = yawErr * _turnGain,
            PitchDelta = pitchErr * _turnGain,
            ThrottleUp = throttleBias > 0.5f ? throttleBias : 0f,
            ThrottleDown = throttleBias < 0.35f ? 0.6f : 0f,
        };

        // Gentle bank into turns.
        if (yawErr > 0.15f)
            intent.RollRight = Math.Clamp(yawErr, 0f, 1f);
        else if (yawErr < -0.15f)
            intent.RollLeft = Math.Clamp(-yawErr, 0f, 1f);

        return intent;
    }
}
