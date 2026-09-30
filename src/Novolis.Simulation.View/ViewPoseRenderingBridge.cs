using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Builds orthonormal observer frames from <see cref="ViewPose"/>.</summary>
public static class ViewPoseRenderingBridge
{
    /// <summary>Converts a pose and aspect ratio into an <see cref="ObserverFrame"/>.</summary>
    public static ObserverFrame ToObserverFrame(in ViewPose pose, float aspectRatio)
    {
        var forward = pose.Target - pose.Position;
        if (forward.LengthSquared() < 1e-12f)
            forward = -Vector3.UnitZ;
        forward = Vector3.Normalize(forward);
        var right = Vector3.Normalize(Vector3.Cross(forward, pose.Up));
        var up = Vector3.Normalize(Vector3.Cross(right, forward));
        return new ObserverFrame(
            pose.Position,
            forward,
            right,
            up,
            pose.FieldOfViewDegrees * (MathF.PI / 180f),
            aspectRatio);
    }
}
