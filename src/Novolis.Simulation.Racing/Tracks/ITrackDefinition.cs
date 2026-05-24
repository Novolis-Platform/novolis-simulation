namespace Novolis.Simulation.Racing.Tracks;

public interface ITrackDefinition
{
    string Id { get; }
    string Name { get; }
    TrackBuildSpec BuildSpec { get; }
}
