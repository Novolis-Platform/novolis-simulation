using Novolis.Simulation.Abstractions;

namespace Novolis.Simulation.Replay;

/// <summary>One resolved simulation step in a stored timeline.</summary>
/// <typeparam name="TState">Snapshot type owned by the host simulation.</typeparam>
/// <param name="StepIndex">Zero-based step index in the timeline.</param>
/// <param name="Step">Clock step metadata (tick and delta).</param>
/// <param name="EndState">State after the step was applied.</param>
/// <param name="StepSeed">Optional deterministic seed used when resolving the step.</param>
public readonly record struct SimulationStepRecord<TState>(
    int StepIndex,
    SimulationStep Step,
    TState EndState,
    int StepSeed = 0)
    where TState : notnull;
