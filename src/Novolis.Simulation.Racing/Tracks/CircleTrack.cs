namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed class CircleTrack : ITrackDefinition
{
    public string Id => "circle";
    public string Name => "Circle";
    public TrackBuildSpec BuildSpec => TrackSpecs.Circle(
        Name, 120, 50, new Vector2(60, 25), 28f, 16f, 4.5, 16, 12, 5);
}
