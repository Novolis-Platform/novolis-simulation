namespace Novolis.Simulation.Racing.Race;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Tracks;

public interface IRaceSimulation
{
    RaceTrack Track { get; }
    IReadOnlyList<IRaceCarController> Controllers { get; }
    RaceWorldState State { get; }
    void Reset();
    void Tick();
}
