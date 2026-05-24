namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed class TrackProgressMap
{
    public required IReadOnlyList<Vector2> Samples { get; init; }
    public required IReadOnlyList<Vector2> Tangents { get; init; }
    public required IReadOnlyList<double> CumulativeArcLengths { get; init; }
    public required double TotalArcLength { get; init; }
}
