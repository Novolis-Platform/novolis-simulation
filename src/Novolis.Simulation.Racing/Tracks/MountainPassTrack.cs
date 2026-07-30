namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Long bottom straight, climb, tight crest, and descent — one dominant hairpin.</summary>
public sealed class MountainPassTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "mountain-pass";
    /// <summary>Name.</summary>
    public string Name => "Mountain pass";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 100,
        rasterHeight: 48,
        trackHalfWidth: 3.5,
        wallThickness: 1.0,
        lapsToFinish: 4,
        controlPoints:
        [
            new(18, 0f, 38), new(72, 0f, 38), new(86, 0f, 26), new(82, 0f, 12),
            new(62, 0f, 8), new(40, 0f, 10), new(22, 0f, 18), new(14, 0f, 28)
        ],
        gateCount: 10);
}
