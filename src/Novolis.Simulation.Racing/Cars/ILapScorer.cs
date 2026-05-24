namespace Novolis.Simulation.Racing.Cars;

using Novolis.Simulation.Racing.Tracks;

public interface ILapScorer
{
    void Update(RaceTrack track, CarState car);
}
