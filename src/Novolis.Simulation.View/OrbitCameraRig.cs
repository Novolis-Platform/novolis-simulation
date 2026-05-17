using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Orbits a smoothed target at yaw/pitch and distance.</summary>
public sealed class OrbitCameraRig
{
    private const float PitchLimit = MathF.PI * 0.49f;

    private Vector3 _smoothedTarget;
    private bool _initialized;

    public Vector3 Target { get; set; }

    public float Distance { get; set; } = 600f;

    public float Yaw { get; set; }

    public float Pitch { get; set; } = 0.35f;

    public float SmoothRate { get; set; } = 10f;

    public float MinDistance { get; set; } = 80f;

    public float MaxDistance { get; set; } = 24_000f;

    public float FieldOfViewDegrees { get; set; } = 52f;

    public void SnapTarget(Vector3 target)
    {
        Target = target;
        _smoothedTarget = target;
        _initialized = true;
    }

    public void ResetSmoothing() => _initialized = false;

    public void AddLookDelta(float deltaYaw, float deltaPitch)
    {
        Yaw += deltaYaw;
        Pitch += deltaPitch;
        Pitch = System.Math.Clamp(Pitch, -0.1f, PitchLimit);
    }

    public void AdjustDistance(float deltaMeters) =>
        Distance = System.Math.Clamp(Distance + deltaMeters, MinDistance, MaxDistance);

    public ViewPose BuildViewPose(float deltaSeconds)
    {
        if (!_initialized)
        {
            _smoothedTarget = Target;
            _initialized = true;
        }
        else
        {
            var t = 1f - MathF.Exp(-SmoothRate * MathF.Max(deltaSeconds, 1e-4f));
            _smoothedTarget = Vector3.Lerp(_smoothedTarget, Target, t);
        }

        var cosP = MathF.Cos(Pitch);
        var offset = new Vector3(
            MathF.Sin(Yaw) * cosP * Distance,
            MathF.Sin(Pitch) * Distance,
            MathF.Cos(Yaw) * cosP * Distance);

        var eye = _smoothedTarget + offset;
        return new ViewPose(eye, _smoothedTarget, Vector3.UnitY, FieldOfViewDegrees);
    }
}
