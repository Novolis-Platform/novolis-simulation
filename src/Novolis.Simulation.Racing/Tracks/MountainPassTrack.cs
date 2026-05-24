namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Long bottom straight, climb, tight crest, and descent — one dominant hairpin.</summary>
public sealed class MountainPassTrack : ITrackDefinition
{
    public string Id => "mountain-pass";
    public string Name => "Mountain pass";
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 100,
        rasterHeight: 48,
        trackHalfWidth: 3.5,
        wallThickness: 1.0,
        lapsToFinish: 4,
        controlPoints:
        [
            new(18, 38), new(72, 38), new(86, 26), new(82, 12),
            new(62, 8), new(40, 10), new(22, 18), new(14, 28)
        ],
        gateCount: 10);
}
