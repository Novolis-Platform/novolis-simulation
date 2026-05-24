namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents ChicaneTrack.</summary>
public sealed class ChicaneTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "chicane";
    /// <summary>Name.</summary>
    public string Name => "Chicane";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec { get; } = new(
        120, 50, 4.0, 1.0, 5,
        new SplineLoop([
            new(15, 25), new(25, 25), new(35, 15), new(45, 15),
            new(55, 25), new(65, 35), new(75, 35), new(85, 25),
            new(95, 25), new(105, 25), new(105, 35), new(95, 40),
            new(50, 40), new(15, 40), new(10, 32)
        ]),
        Enumerable.Range(0, 12).Select(i => i / 12.0).ToArray(),
        0.0);
}
