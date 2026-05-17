namespace Novolis.Simulation.Abstractions;

/// <summary>Mutable simulation state shared by systems.</summary>
public interface ISimulationState
{
    IReadOnlyList<ISimulationObject> Objects { get; }
}
