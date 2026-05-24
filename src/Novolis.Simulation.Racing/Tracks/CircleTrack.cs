namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents CircleTrack.</summary>
public sealed class CircleTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "circle";
    /// <summary>Name.</summary>
    public string Name => "Circle";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec => TrackSpecs.Circle(
        Name, 120, 50, new Vector2(60, 25), 28f, 16f, 4.5, 16, 12, 5);
}
