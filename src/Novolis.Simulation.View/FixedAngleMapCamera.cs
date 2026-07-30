using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>
/// Fixed-angle map / RTS tabletop camera: pan + zoom only, no orbit.
/// Default yaw/pitch match classic C&amp;C diagonal (camera south-east, looking north-west).
/// </summary>
public sealed class FixedAngleMapCamera
{
    /// <summary>Classic C&amp;C diagonal (camera south-east of map, looking north-west).</summary>
    public const float DefaultFixedYaw = MathF.PI * 0.75f;

    /// <summary>~52° elevation — traditional RTS tabletop angle.</summary>
    public const float DefaultFixedPitch = 0.92f;

    /// <summary>Horizontal look angle in radians (fixed for the session unless reassigned).</summary>
    public float FixedYaw { get; set; } = DefaultFixedYaw;

    /// <summary>Vertical look angle in radians (fixed for the session unless reassigned).</summary>
    public float FixedPitch { get; set; } = DefaultFixedPitch;

    /// <summary>Ground-plane pan target (Y forced to 0 on snap / clamp).</summary>
    public Vector3 PanTarget { get; set; }

    /// <summary>Orbit radius from <see cref="PanTarget"/> along the fixed look direction.</summary>
    public float Distance { get; set; } = 26f;

    /// <summary>Minimum zoom distance.</summary>
    public float MinDistance { get; set; } = 14f;

    /// <summary>Maximum zoom distance.</summary>
    public float MaxDistance { get; set; } = 42f;

    /// <summary>Vertical field of view in degrees.</summary>
    public float FieldOfViewDegrees { get; set; } = 42f;

    /// <summary>Snaps the pan target to a world point projected onto the XZ plane.</summary>
    public void SnapTo(Vector3 worldPoint) =>
        PanTarget = new Vector3(worldPoint.X, 0f, worldPoint.Z);

    /// <summary>Pans by a ground-plane delta (Y ignored).</summary>
    public void Pan(Vector3 groundDelta) =>
        PanTarget = new Vector3(PanTarget.X + groundDelta.X, 0f, PanTarget.Z + groundDelta.Z);

    /// <summary>Adjusts zoom distance (e.g. mouse wheel).</summary>
    public void AdjustDistance(float delta) =>
        Distance = System.Math.Clamp(Distance + delta, MinDistance, MaxDistance);

    /// <summary>Clamps <see cref="PanTarget"/> to an axis-aligned ground rectangle.</summary>
    public void ClampPan(float minX, float maxX, float minZ, float maxZ) =>
        PanTarget = new Vector3(
            System.Math.Clamp(PanTarget.X, minX, maxX),
            0f,
            System.Math.Clamp(PanTarget.Z, minZ, maxZ));

    /// <summary>Unit forward on the ground plane along the fixed yaw.</summary>
    public Vector3 GroundForward() =>
        Vector3.Normalize(new Vector3(-MathF.Sin(FixedYaw), 0f, -MathF.Cos(FixedYaw)));

    /// <summary>Unit right on the ground plane along the fixed yaw.</summary>
    public Vector3 GroundRight() =>
        Vector3.Normalize(new Vector3(MathF.Cos(FixedYaw), 0f, -MathF.Sin(FixedYaw)));

    /// <summary>Builds a <see cref="ViewPose"/> from the fixed angles and current pan/zoom.</summary>
    public ViewPose BuildViewPose()
    {
        var cosP = MathF.Cos(FixedPitch);
        var sinP = MathF.Sin(FixedPitch);
        var offset = new Vector3(
            MathF.Sin(FixedYaw) * cosP * Distance,
            sinP * Distance,
            MathF.Cos(FixedYaw) * cosP * Distance);
        var eye = PanTarget + offset;
        return new ViewPose(eye, PanTarget, Vector3.UnitY, FieldOfViewDegrees);
    }

    /// <summary>Projects a screen pixel <c>(X, Y, 0)</c> through the current pose onto the Y=0 ground plane.</summary>
    public Vector3 ScreenToGround(Vector3 screen, int screenW, int screenH)
    {
        var pose = BuildViewPose();
        var nx = (screen.X / screenW - 0.5f) * 2f;
        var ny = (0.5f - screen.Y / screenH) * 2f;
        var aspect = (float)screenW / System.Math.Max(screenH, 1);
        var forward = Vector3.Normalize(pose.Target - pose.Position);
        var right = Vector3.Normalize(Vector3.Cross(forward, pose.Up));
        var up = Vector3.Normalize(Vector3.Cross(right, forward));
        var fovTan = MathF.Tan(pose.FieldOfViewDegrees * MathF.PI / 360f);
        var dir = Vector3.Normalize(forward + right * (nx * fovTan * aspect) + up * (ny * fovTan));

        var origin = pose.Position;
        if (MathF.Abs(dir.Y) < 1e-5f)
            return new Vector3(origin.X, 0f, origin.Z);

        var t = -origin.Y / dir.Y;
        if (t < 0f)
            t = 50f;
        var hit = origin + dir * t;
        return new Vector3(hit.X, 0f, hit.Z);
    }
}
