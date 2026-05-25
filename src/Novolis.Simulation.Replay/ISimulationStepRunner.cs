using Novolis.Simulation.Abstractions;

namespace Novolis.Simulation.Replay;

/// <summary>Advances one simulation step for replay integrity checks.</summary>
/// <typeparam name="TState">Snapshot type owned by the host simulation.</typeparam>
public interface ISimulationStepRunner<TState>
    where TState : notnull
{
    /// <summary>Applies a single step to <paramref name="startState"/>.</summary>
    /// <param name="startState">State at the beginning of the step.</param>
    /// <param name="step">Clock step metadata.</param>
    /// <param name="stepSeed">Deterministic seed for the step.</param>
    /// <returns>State after the step completes.</returns>
    TState RunStep(TState startState, SimulationStep step, int stepSeed);
}
