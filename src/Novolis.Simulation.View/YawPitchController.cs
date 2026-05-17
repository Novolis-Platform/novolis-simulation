using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>
/// Yaw around world +Y and pitch for look up/down (+Y up, motion on XZ).
/// </summary>
public class YawPitchController
{
    private const float PitchLimit = MathF.PI * 0.499f;

    public Vector3 Position { get; set; }

    public float Yaw { get; set; }

    public float Pitch { get; set; }

    public Vector3 GetForwardXZ() => new(MathF.Sin(Yaw), 0f, MathF.Cos(Yaw));

    public Vector3 GetRightXZ() => new(MathF.Cos(Yaw), 0f, -MathF.Sin(Yaw));

    public Vector3 GetLookDirection()
    {
        var cosP = MathF.Cos(Pitch);
        return Vector3.Normalize(new Vector3(MathF.Sin(Yaw) * cosP, MathF.Sin(Pitch), MathF.Cos(Yaw) * cosP));
    }

    public void AddLookDelta(float deltaYaw, float deltaPitch)
    {
        Yaw += deltaYaw;
        Pitch += deltaPitch;
        Pitch = System.Math.Clamp(Pitch, -PitchLimit, PitchLimit);
    }

    public Vector3 GetEyePosition(float eyeHeight = 0f) =>
        Position + new Vector3(0f, eyeHeight, 0f);

    public Vector3 GetLookTarget(float eyeHeight = 0f, float lookDistance = 10f)
    {
        var eye = GetEyePosition(eyeHeight);
        return eye + GetLookDirection() * lookDistance;
    }
}

/// <summary>Obsolete name; use <see cref="YawPitchController"/>.</summary>
[Obsolete("Use YawPitchController. Will be removed in a future release.")]
public sealed class FirstPersonCamera : YawPitchController;
