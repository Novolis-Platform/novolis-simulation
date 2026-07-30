namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents TrackProgressMap.</summary>
public sealed class TrackProgressMap
{
    /// <summary>Samples.</summary>
    public required IReadOnlyList<Vector3> Samples { get; init; }
    /// <summary>Tangents.</summary>
    public required IReadOnlyList<Vector3> Tangents { get; init; }
    /// <summary>CumulativeArcLengths.</summary>
    public required IReadOnlyList<double> CumulativeArcLengths { get; init; }
    /// <summary>TotalArcLength.</summary>
    public required double TotalArcLength { get; init; }
}
