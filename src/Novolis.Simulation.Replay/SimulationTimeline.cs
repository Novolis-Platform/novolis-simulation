namespace Novolis.Simulation.Replay;

/// <summary>Initial snapshot plus an ordered list of resolved steps.</summary>
/// <typeparam name="TState">Snapshot type owned by the host simulation.</typeparam>
/// <param name="Initial">State before the first recorded step.</param>
/// <param name="Steps">Resolved steps in ascending <see cref="SimulationStepRecord{TState}.StepIndex"/> order.</param>
public sealed record SimulationTimeline<TState>(TState Initial, IReadOnlyList<SimulationStepRecord<TState>> Steps)
    where TState : notnull;
