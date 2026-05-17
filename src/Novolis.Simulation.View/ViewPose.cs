using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Observer pose for bridging to a host renderer at compose time.</summary>
public readonly struct ViewPose
{
    public ViewPose(Vector3 position, Vector3 target, Vector3 up, float fieldOfViewDegrees)
    {
        Position = position;
        Target = target;
        Up = up;
        FieldOfViewDegrees = fieldOfViewDegrees;
    }

    public Vector3 Position { get; }

    public Vector3 Target { get; }

    public Vector3 Up { get; }

    public float FieldOfViewDegrees { get; }
}
