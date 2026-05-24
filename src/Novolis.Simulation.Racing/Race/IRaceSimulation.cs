namespace Novolis.Simulation.Racing.Race;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Tracks;

/// <summary>Represents IRaceSimulation.</summary>
public interface IRaceSimulation
{
    /// <summary>Track.</summary>
    RaceTrack Track { get; }
    /// <summary>Controllers.</summary>
    IReadOnlyList<IRaceCarController> Controllers { get; }
    /// <summary>State.</summary>
    RaceWorldState State { get; }
    /// <summary>Reset operation.</summary>
    void Reset();
    /// <summary>Tick operation.</summary>
    void Tick();
}
