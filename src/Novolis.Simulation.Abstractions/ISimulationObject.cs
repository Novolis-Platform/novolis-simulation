namespace Novolis.Simulation.Abstractions;

/// <summary>Identified entity in the simulation world.</summary>
public interface ISimulationObject
{
    /// <summary>Id.</summary>
    Guid Id { get; }
}
