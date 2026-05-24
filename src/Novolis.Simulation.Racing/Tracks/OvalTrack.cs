namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents OvalTrack.</summary>
public sealed class OvalTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "oval";
    /// <summary>Name.</summary>
    public string Name => "Oval";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec => TrackSpecs.Circle(
        Name, 120, 50, new Vector2(60, 25), 36f, 14f, 5.0, 20, 14, 6);
}
