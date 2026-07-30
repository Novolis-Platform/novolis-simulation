namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Snake + chicane layout on a reduced grid (similar spirit to <see cref="ChicaneTrack"/>).</summary>
public sealed class ShortChicaneTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "short-chicane";
    /// <summary>Name.</summary>
    public string Name => "Short chicane";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 92,
        rasterHeight: 40,
        trackHalfWidth: 3.6,
        wallThickness: 1.0,
        lapsToFinish: 4,
        controlPoints:
        [
            new(14, 0f, 20), new(24, 0f, 20), new(34, 0f, 12), new(46, 0f, 12),
            new(54, 0f, 20), new(64, 0f, 28), new(74, 0f, 28), new(82, 0f, 20),
            new(82, 0f, 12), new(72, 0f, 8), new(50, 0f, 8), new(28, 0f, 14),
            new(14, 0f, 18)
        ],
        gateCount: 10);
}
