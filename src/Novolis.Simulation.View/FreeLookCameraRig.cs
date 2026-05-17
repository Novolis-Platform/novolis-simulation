using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Free-flying camera with yaw/pitch look and WASD-style movement on world axes.</summary>
public sealed class FreeLookCameraRig
{
    private const float PitchLimit = MathF.PI * 0.49f;

    public Vector3 Position { get; set; }

    public float Yaw { get; set; }

    public float Pitch { get; set; }

    public float FieldOfViewDegrees { get; set; } = 52f;

    public void AddLookDelta(float deltaYaw, float deltaPitch)
    {
        Yaw += deltaYaw;
        Pitch += deltaPitch;
        Pitch = System.Math.Clamp(Pitch, -PitchLimit, PitchLimit);
    }

    public void MoveAlongLook(Vector3 worldDelta)
    {
        if (worldDelta.LengthSquared() < 1e-10f)
            return;

        Position += worldDelta;
    }

    public Vector3 GetLookDirection()
    {
        var cosP = MathF.Cos(Pitch);
        return Vector3.Normalize(new Vector3(MathF.Sin(Yaw) * cosP, MathF.Sin(Pitch), MathF.Cos(Yaw) * cosP));
    }

    public ViewPose BuildViewPose(float lookDistance = 50f) =>
        new(
            Position,
            Position + GetLookDirection() * lookDistance,
            Vector3.UnitY,
            FieldOfViewDegrees);
}
