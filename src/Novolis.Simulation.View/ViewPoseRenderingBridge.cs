using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>
/// Maps simulation <see cref="ViewPose"/> to rendering-friendly observer parameters without referencing Rendering packages.
/// Dogfooding hosts convert the result to <c>CameraSnapshot</c> via PackageReference to Rendering.Runtime.
/// </summary>
public readonly record struct ObserverFrame(
    Vector3 Position,
    Vector3 Forward,
    Vector3 Right,
    Vector3 Up,
    float VerticalFovRadians,
    float AspectRatio);

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
