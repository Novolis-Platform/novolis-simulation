using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

/// <summary>Fixed-size feature vector for craft observations (ML / telemetry).</summary>
public static class CraftObservationFeatures
{
    public const int Size = 14;

    /// <summary>
    /// Writes a deterministic observation: local aim error, range, speed, threats, escort offset.
    /// Values are roughly in [-1, 1] or [0, 1].
    /// </summary>
    public static void Write(in CraftObservation observation, Span<float> destination)
    {
        if (destination.Length < Size)
            throw new ArgumentException($"Destination must be at least {Size}.", nameof(destination));

        var self = observation.Self;
        destination.Clear();

        float yawErr = 0f, pitchErr = 0f, range01 = 0f, closing = 0f;
        if (observation.TargetPosition is { } target)
        {
            var lead = target;
            if (observation.TargetVelocity is { } tv)
            {
                var d = Vector3.Distance(self.Position, target);
                lead = target + tv * (d / 95f);
            }

            CrewIntentComposer.AimError(self, lead, out yawErr, out pitchErr);
            var dist = Vector3.Distance(self.Position, lead);
            range01 = Math.Clamp(dist / 90f, 0f, 1f);
            var to = lead - self.Position;
            if (to.LengthSquared() > 1e-4f)
                closing = Math.Clamp(Vector3.Dot(self.Velocity, Vector3.Normalize(to)) / 40f, -1f, 1f);
        }

        CrewIntentComposer.AimError(self, observation.EscortAnchor, out var escortYaw, out var escortPitch);
        var escortDist = Vector3.Distance(self.Position, observation.EscortAnchor);

        destination[0] = yawErr;
        destination[1] = pitchErr;
        destination[2] = range01;
        destination[3] = closing;
        destination[4] = Math.Clamp(self.Throttle01 * 2f - 1f, -1f, 1f);
        destination[5] = Math.Clamp(self.Speed / Math.Max(1f, self.Profile.MaxSpeed), 0f, 1f) * 2f - 1f;
        destination[6] = Math.Clamp(self.Hull / Math.Max(0.01f, self.Profile.MaxHull), 0f, 1f);
        destination[7] = Math.Clamp(self.Shield / Math.Max(0.01f, self.Profile.MaxShield), 0f, 1f);
        destination[8] = Math.Clamp(observation.ActiveThreats / 6f, 0f, 1f);
        destination[9] = escortYaw;
        destination[10] = escortPitch;
        destination[11] = Math.Clamp(escortDist / 120f, 0f, 1f);
        destination[12] = observation.TargetPosition.HasValue ? 1f : 0f;
        destination[13] = self.Profile.Role == CraftRole.Freighter ? -1f : 1f;
    }
}
