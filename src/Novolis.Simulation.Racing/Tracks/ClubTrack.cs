namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed class ClubTrack : ITrackDefinition
{
    public string Id => "club";
    public string Name => "Club";
    public TrackBuildSpec BuildSpec { get; } = new(
        110, 50, 3.5, 1.0, 5,
        new SplineLoop([
            new(20, 25), new(30, 14), new(48, 10), new(70, 12),
            new(90, 22), new(92, 34), new(78, 40), new(60, 38),
            new(46, 30), new(38, 34), new(26, 38), new(18, 32)
        ]),
        Enumerable.Range(0, 12).Select(i => i / 12.0).ToArray(),
        0.0);
}
