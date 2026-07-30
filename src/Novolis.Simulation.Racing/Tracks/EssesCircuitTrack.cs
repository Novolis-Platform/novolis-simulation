namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Flowing S-bends with fair straight exits — rewards smooth steering.</summary>
public sealed class EssesCircuitTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "esses";
    /// <summary>Name.</summary>
    public string Name => "Esses";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 96,
        rasterHeight: 42,
        trackHalfWidth: 3.8,
        wallThickness: 1.0,
        lapsToFinish: 4,
        controlPoints:
        [
            new(14, 0f, 21), new(28, 0f, 10), new(48, 0f, 16), new(68, 0f, 10),
            new(84, 0f, 21), new(68, 0f, 32), new(48, 0f, 26), new(28, 0f, 32)
        ],
        gateCount: 10);
}
