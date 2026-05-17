namespace Novolis.Simulation.Abstractions;

/// <summary>Runs one simulation step against shared state.</summary>
public interface ISimulationSystem
{
    void Step(ISimulationState state, in SimulationStep step);
}
