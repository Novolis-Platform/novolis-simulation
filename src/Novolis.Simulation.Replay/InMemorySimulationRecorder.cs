using Novolis.Simulation.Abstractions;

namespace Novolis.Simulation.Replay;

/// <summary>Builds an in-memory <see cref="SimulationTimeline{TState}"/> while a simulation runs.</summary>
/// <typeparam name="TState">Snapshot type owned by the host simulation.</typeparam>
public sealed class InMemorySimulationRecorder<TState>
    where TState : notnull
{
    private readonly List<SimulationStepRecord<TState>> _steps = [];
    private TState _initial = default!;
    private bool _hasInitial;

    /// <summary>Captures the state before the first step is recorded.</summary>
    /// <param name="initial">Initial snapshot.</param>
    public void SetInitial(TState initial)
    {
        _initial = initial;
        _hasInitial = true;
        _steps.Clear();
    }

    /// <summary>Appends a resolved step.</summary>
    /// <param name="step">Clock step metadata.</param>
    /// <param name="endState">State after the step.</param>
    /// <param name="stepSeed">Optional deterministic seed.</param>
    public void RecordStep(SimulationStep step, TState endState, int stepSeed = 0)
    {
        if (!_hasInitial)
            throw new InvalidOperationException("Call SetInitial before RecordStep.");

        _steps.Add(new SimulationStepRecord<TState>(_steps.Count, step, endState, stepSeed));
    }

    /// <summary>Builds the timeline from captured steps.</summary>
    /// <returns>A read-only timeline.</returns>
    public SimulationTimeline<TState> Build() =>
        _hasInitial
            ? new SimulationTimeline<TState>(_initial, _steps.ToArray())
            : throw new InvalidOperationException("Call SetInitial before Build.");
}
