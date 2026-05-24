using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Observer pose for bridging to a host renderer at compose time.</summary>
public readonly struct ViewPose
{
    /// <summary>ViewPose operation.</summary>
    public ViewPose(Vector3 position, Vector3 target, Vector3 up, float fieldOfViewDegrees)
    {
        Position = position;
        Target = target;
        Up = up;
        FieldOfViewDegrees = fieldOfViewDegrees;
    }

    /// <summary>Position.</summary>
    public Vector3 Position { get; }

    /// <summary>Target.</summary>
    public Vector3 Target { get; }

    /// <summary>Up.</summary>
    public Vector3 Up { get; }

    /// <summary>FieldOfViewDegrees.</summary>
    public float FieldOfViewDegrees { get; }
}
