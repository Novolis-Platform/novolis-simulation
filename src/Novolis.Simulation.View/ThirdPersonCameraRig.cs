using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>
/// Third-person boom camera following a target with shared yaw/pitch.
/// Optional <see cref="CollisionProbe"/> shrinks boom distance when obstructed.
/// </summary>
public sealed class ThirdPersonCameraRig : IViewController
{
    private const float PitchLimit = MathF.PI * 0.49f;

    /// <summary>Creates a TPS boom camera.</summary>
    public ThirdPersonCameraRig(
        YawPitchController look,
        float boomDistance = 4f,
        float fieldOfViewDegrees = 60f)
    {
        Look = look ?? throw new ArgumentNullException(nameof(look));
        BoomDistance = boomDistance;
        FieldOfViewDegrees = fieldOfViewDegrees;
        MinPitch = -0.2f;
        MaxPitch = PitchLimit;
    }

    /// <summary>Shared look (yaw/pitch). Feet position is the follow target.</summary>
    public YawPitchController Look { get; }

    /// <summary>Ideal camera distance behind the target.</summary>
    public float BoomDistance { get; set; }

    /// <summary>Minimum boom after collision pull-in.</summary>
    public float MinBoomDistance { get; set; } = 0.5f;

    /// <summary>Height offset above the follow target.</summary>
    public float ShoulderHeight { get; set; } = 1.4f;

    /// <summary>Pitch clamp low (radians).</summary>
    public float MinPitch { get; set; }

    /// <summary>Pitch clamp high (radians).</summary>
    public float MaxPitch { get; set; }

    /// <summary>Vertical FOV.</summary>
    public float FieldOfViewDegrees { get; set; }

    /// <summary>
    /// Optional probe: given (target, desiredEye) returns max allowed distance from target to eye.
    /// </summary>
    public Func<Vector3, Vector3, float>? CollisionProbe { get; set; }

    /// <inheritdoc />
    public ViewPose Pose { get; private set; }

    /// <summary>Applies look deltas and clamps pitch for TPS.</summary>
    public void ApplyLook(in LookIntent intent)
    {
        Look.AddLookDelta(intent.DeltaYaw, intent.DeltaPitch);
        Look.Pitch = Math.Clamp(Look.Pitch, MinPitch, MaxPitch);
        if (MathF.Abs(intent.ZoomDelta) > 1e-6f)
            BoomDistance = Math.Max(MinBoomDistance, BoomDistance + intent.ZoomDelta);
    }

    /// <inheritdoc />
    public void Tick(float deltaSeconds)
    {
        _ = deltaSeconds;
        var target = Look.Position + new Vector3(0f, ShoulderHeight, 0f);
        var cosP = MathF.Cos(Look.Pitch);
        var offsetDir = new Vector3(
            MathF.Sin(Look.Yaw) * cosP,
            MathF.Sin(Look.Pitch),
            MathF.Cos(Look.Yaw) * cosP);
        // Camera sits opposite look direction (behind the character).
        offsetDir = -offsetDir;
        if (offsetDir.LengthSquared() < 1e-8f)
            offsetDir = new Vector3(0f, 0f, -1f);
        else
            offsetDir = Vector3.Normalize(offsetDir);

        var distance = BoomDistance;
        var desiredEye = target + offsetDir * distance;
        if (CollisionProbe is { } probe)
        {
            var allowed = probe(target, desiredEye);
            distance = Math.Clamp(allowed, MinBoomDistance, BoomDistance);
            desiredEye = target + offsetDir * distance;
        }

        Pose = new ViewPose(desiredEye, target, Vector3.UnitY, FieldOfViewDegrees);
    }
}
