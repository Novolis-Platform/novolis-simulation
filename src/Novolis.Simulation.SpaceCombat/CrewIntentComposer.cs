using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

/// <summary>Merges human and AI intents based on <see cref="CrewStation"/>.</summary>
public static class CrewIntentComposer
{
    /// <summary>
    /// Combines player input with AI pilot / gunner intents.
    /// Transfer is always taken from the player.
    /// </summary>
    public static FlightIntent Compose(
        CrewStation station,
        in FlightIntent player,
        in FlightIntent aiPilot,
        in FlightIntent aiGunner)
    {
        return station switch
        {
            CrewStation.Pilot => new FlightIntent
            {
                // Human flies the stick; AI supplies fire + mild aim assist.
                YawDelta = player.YawDelta + aiGunner.YawDelta * 0.4f,
                PitchDelta = player.PitchDelta + aiGunner.PitchDelta * 0.4f,
                RollLeft = player.RollLeft,
                RollRight = player.RollRight,
                ThrottleUp = player.ThrottleUp,
                ThrottleDown = player.ThrottleDown,
                Fire = player.Fire || aiGunner.Fire,
                Transfer = player.Transfer,
            },
            CrewStation.Gunner => new FlightIntent
            {
                // AI flies; human aims and fires (mouse overrides course).
                YawDelta = aiPilot.YawDelta * 0.65f + player.YawDelta,
                PitchDelta = aiPilot.PitchDelta * 0.65f + player.PitchDelta,
                RollLeft = aiPilot.RollLeft,
                RollRight = aiPilot.RollRight,
                ThrottleUp = aiPilot.ThrottleUp,
                ThrottleDown = aiPilot.ThrottleDown,
                Fire = player.Fire,
                Transfer = player.Transfer,
            },
            _ => player,
        };
    }

    /// <summary>Builds a unit-length aim error in craft local yaw/pitch space (approx).</summary>
    public static void AimError(
        CraftState self,
        Vector3 worldTarget,
        out float yawError,
        out float pitchError)
    {
        var to = worldTarget - self.Position;
        if (to.LengthSquared() < 1e-4f)
        {
            yawError = 0f;
            pitchError = 0f;
            return;
        }

        var desired = Vector3.Normalize(to);
        var forward = self.Forward;
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        if (right.LengthSquared() < 1e-4f)
            right = Vector3.UnitX;
        var up = Vector3.Normalize(Vector3.Cross(right, forward));

        yawError = Math.Clamp(Vector3.Dot(desired, right), -1f, 1f);
        pitchError = Math.Clamp(Vector3.Dot(desired, up), -1f, 1f);
    }
}
