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
