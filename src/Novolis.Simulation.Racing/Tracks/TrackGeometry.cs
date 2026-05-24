namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed class TrackGeometry
{
    public required IReadOnlyList<Vector2> LeftBoundary { get; init; }
    public required IReadOnlyList<Vector2> RightBoundary { get; init; }
    public required double HalfWidth { get; init; }
}
