namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Mid-size ellipse; wider straights feel than <see cref="CircleTrack"/> but smaller than <see cref="OvalTrack"/>.</summary>
public sealed class CompactOvalTrack : ITrackDefinition
{
    public string Id => "compact-oval";
    public string Name => "Compact oval";
    public TrackBuildSpec BuildSpec => TrackSpecs.Circle(
        Name,
        rasterWidth: 88,
        rasterHeight: 44,
        center: new Vector2(44, 22),
        radiusX: 24f,
        radiusY: 13f,
        trackHalfWidth: 4.0,
        controlPoints: 18,
        gates: 10,
        lapsToFinish: 4);
}
