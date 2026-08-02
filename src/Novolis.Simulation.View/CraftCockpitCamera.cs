using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Cockpit / chase pose from craft position, forward, and roll.</summary>
public static class CraftCamera
{
    /// <summary>First-person cockpit pose with roll applied to the up vector.</summary>
    public static ViewPose Cockpit(
        Vector3 position,
        Vector3 forward,
        float rollRadians,
        float eyeHeight = 0.35f,
        float lookDistance = 10f,
        float fieldOfViewDegrees = 72f)
    {
        var eye = position + new Vector3(0, eyeHeight, 0);
        var target = eye + forward * lookDistance;
        var worldUp = Vector3.UnitY;
        var right = Vector3.Normalize(Vector3.Cross(forward, worldUp));
        if (right.LengthSquared() < 1e-6f)
            right = Vector3.UnitX;
        var up = Vector3.Normalize(Vector3.Cross(right, forward));
        var rolledUp = Vector3.Normalize(up * MathF.Cos(rollRadians) + right * MathF.Sin(rollRadians));
        return new ViewPose(eye, target, rolledUp, fieldOfViewDegrees);
    }

    /// <summary>Third-person chase camera aft of the craft.</summary>
    public static ViewPose ChaseAft(
        Vector3 position,
        Vector3 forward,
        float rollRadians,
        float distance = 14f,
        float height = 3.5f,
        float fieldOfViewDegrees = 70f)
    {
        var back = -forward;
        var eye = position + back * distance + new Vector3(0, height, 0);
        var target = position + forward * 6f;
        var worldUp = Vector3.UnitY;
        var right = Vector3.Normalize(Vector3.Cross(forward, worldUp));
        if (right.LengthSquared() < 1e-6f)
            right = Vector3.UnitX;
        var up = Vector3.Normalize(Vector3.Cross(right, forward));
        var rolledUp = Vector3.Normalize(up * MathF.Cos(rollRadians * 0.35f) + right * MathF.Sin(rollRadians * 0.35f));
        return new ViewPose(eye, target, rolledUp, fieldOfViewDegrees);
    }
}
