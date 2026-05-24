namespace Novolis.Simulation.Racing.Tracks;

public interface ITrackBuilder
{
    RaceTrack Build(ITrackDefinition definition);
}
