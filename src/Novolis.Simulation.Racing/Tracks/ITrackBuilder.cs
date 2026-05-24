namespace Novolis.Simulation.Racing.Tracks;

/// <summary>Represents ITrackBuilder.</summary>
public interface ITrackBuilder
{
    /// <summary>Builds the configured instance.</summary>
    RaceTrack Build(ITrackDefinition definition);
}
