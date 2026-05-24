namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Small circular course for fast iteration, headless CI, and tight raster budgets.</summary>
public sealed class MicroCircleTrack : ITrackDefinition
{
    public string Id => "micro-circle";
    public string Name => "Micro circle";
    public TrackBuildSpec BuildSpec => TrackSpecs.Circle(
        Name,
        rasterWidth: 56,
        rasterHeight: 28,
        center: new Vector2(28, 14),
        radiusX: 11f,
        radiusY: 8f,
        trackHalfWidth: 3.2,
        controlPoints: 12,
        gates: 8,
        lapsToFinish: 3);
}
