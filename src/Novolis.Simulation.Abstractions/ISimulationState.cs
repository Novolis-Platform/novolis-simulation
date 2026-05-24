namespace Novolis.Simulation.Abstractions;

/// <summary>Mutable simulation state shared by systems.</summary>
public interface ISimulationState
{
    /// <summary>Objects.</summary>
    IReadOnlyList<ISimulationObject> Objects { get; }
}
