namespace Novolis.Simulation.Racing.Tracks;

/// <summary>Represents ITrackDefinition.</summary>
public interface ITrackDefinition
{
    /// <summary>Id.</summary>
    string Id { get; }
    /// <summary>Name.</summary>
    string Name { get; }
    /// <summary>BuildSpec.</summary>
    TrackBuildSpec BuildSpec { get; }
}
