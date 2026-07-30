namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents ClubTrack.</summary>
public sealed class ClubTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "club";
    /// <summary>Name.</summary>
    public string Name => "Club";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec { get; } = new(
        110, 50, 3.5, 1.0, 5,
        new SplineLoop([
            new(20, 0f, 25), new(30, 0f, 14), new(48, 0f, 10), new(70, 0f, 12),
            new(90, 0f, 22), new(92, 0f, 34), new(78, 0f, 40), new(60, 0f, 38),
            new(46, 0f, 30), new(38, 0f, 34), new(26, 0f, 38), new(18, 0f, 32)
        ]),
        Enumerable.Range(0, 12).Select(i => i / 12.0).ToArray(),
        0.0);
}
