namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed class RaceTrack
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required TrackCell[,] Cells { get; init; }
    public required IReadOnlyList<Vector2> CenterLineSamples { get; init; }
    public required IReadOnlyList<TrackGate> Gates { get; init; }
    public required TrackStartPose StartPose { get; init; }
    public required TrackProgressMap ProgressMap { get; init; }
    public required TrackGeometry Geometry { get; init; }
}
