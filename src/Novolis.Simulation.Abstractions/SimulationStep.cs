namespace Novolis.Simulation.Abstractions;

/// <summary>One fixed or variable simulation tick.</summary>
public readonly struct SimulationStep(double deltaSeconds, ulong tick)
{
    public double DeltaSeconds { get; } = deltaSeconds;
    public ulong Tick { get; } = tick;
}
