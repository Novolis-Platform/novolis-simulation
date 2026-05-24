namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Flowing S-bends with fair straight exits — rewards smooth steering.</summary>
public sealed class EssesCircuitTrack : ITrackDefinition
{
    public string Id => "esses";
    public string Name => "Esses";
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 96,
        rasterHeight: 42,
        trackHalfWidth: 3.8,
        wallThickness: 1.0,
        lapsToFinish: 4,
        controlPoints:
        [
            new(14, 21), new(28, 10), new(48, 16), new(68, 10),
            new(84, 21), new(68, 32), new(48, 26), new(28, 32)
        ],
        gateCount: 10);
}
