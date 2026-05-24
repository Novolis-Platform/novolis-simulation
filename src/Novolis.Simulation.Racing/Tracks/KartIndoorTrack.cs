namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Tight indoor-style loop; short lap, good for dense traffic experiments.</summary>
public sealed class KartIndoorTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "kart-indoor";
    /// <summary>Name.</summary>
    public string Name => "Kart indoor";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec => TrackSpecs.Polyline(
        rasterWidth: 68,
        rasterHeight: 34,
        trackHalfWidth: 3.0,
        wallThickness: 1.0,
        lapsToFinish: 3,
        controlPoints:
        [
            new(16, 16), new(44, 10), new(56, 20), new(50, 24),
            new(34, 26), new(22, 24), new(12, 20)
        ],
        gateCount: 8);
}
