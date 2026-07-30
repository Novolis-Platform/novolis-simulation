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
            new(15, 0f, 25), new(25, 0f, 25), new(35, 0f, 15), new(45, 0f, 15),
            new(55, 0f, 25), new(65, 0f, 35), new(75, 0f, 35), new(85, 0f, 25),
            new(95, 0f, 25), new(105, 0f, 25), new(105, 0f, 35), new(95, 0f, 40),
            new(50, 0f, 40), new(15, 0f, 40), new(10, 0f, 32)
        ]),
        Enumerable.Range(0, 12).Select(i => i / 12.0).ToArray(),
        0.0);
}
