using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>First-person eye camera driven by a shared <see cref="YawPitchController"/>.</summary>
public sealed class FirstPersonCameraRig : IViewController
{
    /// <summary>Creates a rig bound to <paramref name="look"/> (shared with locomotion / director).</summary>
    public FirstPersonCameraRig(YawPitchController look, float eyeHeight = 1.7f, float fieldOfViewDegrees = 70f)
    {
        Look = look ?? throw new ArgumentNullException(nameof(look));
        EyeHeight = eyeHeight;
        FieldOfViewDegrees = fieldOfViewDegrees;
    }

    /// <summary>Shared yaw/pitch + feet position.</summary>
    public YawPitchController Look { get; }

    /// <summary>Eye height above <see cref="YawPitchController.Position"/>.</summary>
    public float EyeHeight { get; set; }

    /// <summary>Vertical field of view in degrees.</summary>
    public float FieldOfViewDegrees { get; set; }

    /// <summary>Look-at distance used when building <see cref="ViewPose.Target"/>.</summary>
    public float LookDistance { get; set; } = 10f;

    /// <inheritdoc />
    public ViewPose Pose { get; private set; }

    /// <summary>Applies a look intent (no tick required for look-only).</summary>
    public void ApplyLook(in LookIntent intent) =>
        Look.AddLookDelta(intent.DeltaYaw, intent.DeltaPitch);

    /// <inheritdoc />
    public void Tick(float deltaSeconds)
    {
        _ = deltaSeconds;
        var eye = Look.GetEyePosition(EyeHeight);
        var target = Look.GetLookTarget(EyeHeight, LookDistance);
        Pose = new ViewPose(eye, target, Vector3.UnitY, FieldOfViewDegrees);
    }
}
