namespace Novolis.Simulation.Abstractions;

/// <summary>One fixed or variable simulation tick.</summary>
public readonly struct SimulationStep(double deltaSeconds, ulong tick)
{
    /// <summary>DeltaSeconds.</summary>
    public double DeltaSeconds { get; } = deltaSeconds;
    /// <summary>Tick.</summary>
    public ulong Tick { get; } = tick;
}
