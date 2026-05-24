namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Extended lap with gentle bends and a late tightening — medium commitment braking.</summary>
public sealed class RiverSprintTrack : ITrackDefinition
{
    public string Id => "river-sprint";
    public string Name => "River sprint";
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 104,
        rasterHeight: 46,
        trackHalfWidth: 4.0,
        wallThickness: 1.0,
        lapsToFinish: 5,
        controlPoints:
        [
            new(16, 23), new(36, 12), new(58, 14), new(78, 10),
            new(94, 22), new(82, 34), new(58, 32), new(38, 36),
            new(18, 30)
        ],
        gateCount: 12);
}
