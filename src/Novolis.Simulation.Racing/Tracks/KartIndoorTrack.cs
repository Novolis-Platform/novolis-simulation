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
            new(16, 0f, 16), new(44, 0f, 10), new(56, 0f, 20), new(50, 0f, 24),
            new(34, 0f, 26), new(22, 0f, 24), new(12, 0f, 20)
        ],
        gateCount: 8);
}
