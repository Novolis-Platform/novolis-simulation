namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents RaceTrack.</summary>
public sealed class RaceTrack
{
    /// <summary>Id.</summary>
    public required string Id { get; init; }
    /// <summary>Name.</summary>
    public required string Name { get; init; }
    /// <summary>Width.</summary>
    public required int Width { get; init; }
    /// <summary>Height.</summary>
    public required int Height { get; init; }
    /// <summary>Cells.</summary>
    public required TrackCell[,] Cells { get; init; }
    /// <summary>CenterLineSamples.</summary>
    public required IReadOnlyList<Vector2> CenterLineSamples { get; init; }
    /// <summary>Gates.</summary>
    public required IReadOnlyList<TrackGate> Gates { get; init; }
    /// <summary>StartPose.</summary>
    public required TrackStartPose StartPose { get; init; }
    /// <summary>ProgressMap.</summary>
    public required TrackProgressMap ProgressMap { get; init; }
    /// <summary>Geometry.</summary>
    public required TrackGeometry Geometry { get; init; }
}
