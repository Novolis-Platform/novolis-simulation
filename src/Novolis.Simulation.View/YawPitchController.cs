using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>
/// Yaw around world +Y and pitch for look up/down (+Y up, motion on XZ).
/// </summary>
public class YawPitchController
{
    private const float PitchLimit = MathF.PI * 0.499f;

    /// <summary>Position.</summary>
    public Vector3 Position { get; set; }

    /// <summary>Yaw.</summary>
    public float Yaw { get; set; }

    /// <summary>Pitch.</summary>
    public float Pitch { get; set; }

    /// <summary>Gets ForwardXZ.</summary>
    public Vector3 GetForwardXZ() => new(MathF.Sin(Yaw), 0f, MathF.Cos(Yaw));

    /// <summary>Gets RightXZ.</summary>
    public Vector3 GetRightXZ() => new(MathF.Cos(Yaw), 0f, -MathF.Sin(Yaw));

    /// <summary>Gets LookDirection.</summary>
    public Vector3 GetLookDirection()
    {
        var cosP = MathF.Cos(Pitch);
        return Vector3.Normalize(new Vector3(MathF.Sin(Yaw) * cosP, MathF.Sin(Pitch), MathF.Cos(Yaw) * cosP));
    }

    /// <summary>Adds an item.</summary>
    public void AddLookDelta(float deltaYaw, float deltaPitch)
    {
        Yaw += deltaYaw;
        Pitch += deltaPitch;
        Pitch = System.Math.Clamp(Pitch, -PitchLimit, PitchLimit);
    }

    /// <summary>Gets EyePosition.</summary>
    public Vector3 GetEyePosition(float eyeHeight = 0f) =>
        Position + new Vector3(0f, eyeHeight, 0f);

    /// <summary>Gets LookTarget.</summary>
    public Vector3 GetLookTarget(float eyeHeight = 0f, float lookDistance = 10f)
    {
        var eye = GetEyePosition(eyeHeight);
        return eye + GetLookDirection() * lookDistance;
    }
}
