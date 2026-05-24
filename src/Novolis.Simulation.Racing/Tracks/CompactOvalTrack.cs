namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Mid-size ellipse; wider straights feel than <see cref="CircleTrack"/> but smaller than <see cref="OvalTrack"/>.</summary>
public sealed class CompactOvalTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "compact-oval";
    /// <summary>Name.</summary>
    public string Name => "Compact oval";
    /// <summary>BuildSpec.</summary>
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
