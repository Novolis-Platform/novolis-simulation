namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed class OvalTrack : ITrackDefinition
{
    public string Id => "oval";
    public string Name => "Oval";
    public TrackBuildSpec BuildSpec => TrackSpecs.Circle(
        Name, 120, 50, new Vector2(60, 25), 36f, 14f, 5.0, 20, 14, 6);
}
