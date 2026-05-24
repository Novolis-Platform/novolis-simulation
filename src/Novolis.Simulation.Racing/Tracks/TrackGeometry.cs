namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents TrackGeometry.</summary>
public sealed class TrackGeometry
{
    /// <summary>LeftBoundary.</summary>
    public required IReadOnlyList<Vector2> LeftBoundary { get; init; }
    /// <summary>RightBoundary.</summary>
    public required IReadOnlyList<Vector2> RightBoundary { get; init; }
    /// <summary>HalfWidth.</summary>
    public required double HalfWidth { get; init; }
}
