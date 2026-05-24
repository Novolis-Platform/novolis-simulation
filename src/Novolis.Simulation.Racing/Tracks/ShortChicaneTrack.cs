namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Snake + chicane layout on a reduced grid (similar spirit to <see cref="ChicaneTrack"/>).</summary>
public sealed class ShortChicaneTrack : ITrackDefinition
{
    public string Id => "short-chicane";
    public string Name => "Short chicane";
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 92,
        rasterHeight: 40,
        trackHalfWidth: 3.6,
        wallThickness: 1.0,
        lapsToFinish: 4,
        controlPoints:
        [
            new(14, 20), new(24, 20), new(34, 12), new(46, 12),
            new(54, 20), new(64, 28), new(74, 28), new(82, 20),
            new(82, 12), new(72, 8), new(50, 8), new(28, 14),
            new(14, 18)
        ],
        gateCount: 10);
}
