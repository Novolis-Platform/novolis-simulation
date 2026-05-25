using Novolis.Simulation.Abstractions;

namespace Novolis.Simulation.Replay;

/// <summary>Deterministic queries over a stored <see cref="SimulationTimeline{TState}"/>.</summary>
public static class ReplayPlayback
{
    /// <summary>Returns the state at the end of a step index, or the initial state when <paramref name="stepIndex"/> is negative.</summary>
    /// <typeparam name="TState">Snapshot type owned by the host simulation.</typeparam>
    /// <param name="timeline">Stored timeline.</param>
    /// <param name="stepIndex">Zero-based step index, or -1 for <see cref="SimulationTimeline{TState}.Initial"/>.</param>
    /// <returns>Snapshot at the end of the requested step.</returns>
    public static TState GetEndStateAt<TState>(SimulationTimeline<TState> timeline, int stepIndex)
        where TState : notnull
    {
        if (stepIndex < 0)
            return timeline.Initial;

        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stepIndex, timeline.Steps.Count);
        return timeline.Steps[stepIndex].EndState;
    }

    /// <summary>Re-runs one stored step and compares the result to the recorded end state.</summary>
    /// <typeparam name="TState">Snapshot type owned by the host simulation.</typeparam>
    /// <param name="timeline">Stored timeline.</param>
    /// <param name="stepIndex">Zero-based step index.</param>
    /// <param name="runner">Step resolver used for integrity checks.</param>
    /// <param name="equality">Equality comparer for <typeparamref name="TState"/>.</param>
    /// <returns><see langword="true"/> when the recomputed end state matches the recording.</returns>
    public static bool VerifyStep<TState>(
        SimulationTimeline<TState> timeline,
        int stepIndex,
        ISimulationStepRunner<TState> runner,
        IEqualityComparer<TState>? equality = null)
        where TState : notnull
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(stepIndex, timeline.Steps.Count);
        equality ??= EqualityComparer<TState>.Default;

        var record = timeline.Steps[stepIndex];
        var start = stepIndex == 0 ? timeline.Initial : timeline.Steps[stepIndex - 1].EndState;
        var recomputed = runner.RunStep(start, record.Step, record.StepSeed);
        return equality.Equals(recomputed, record.EndState);
    }

    /// <summary>Re-runs every stored step.</summary>
    /// <typeparam name="TState">Snapshot type owned by the host simulation.</typeparam>
    /// <param name="timeline">Stored timeline.</param>
    /// <param name="runner">Step resolver used for integrity checks.</param>
    /// <param name="equality">Equality comparer for <typeparamref name="TState"/>.</param>
    /// <returns><see langword="true"/> when all steps match their recordings.</returns>
    public static bool VerifyAllSteps<TState>(
        SimulationTimeline<TState> timeline,
        ISimulationStepRunner<TState> runner,
        IEqualityComparer<TState>? equality = null)
        where TState : notnull
    {
        for (var i = 0; i < timeline.Steps.Count; i++)
        {
            if (!VerifyStep(timeline, i, runner, equality))
                return false;
        }

        return true;
    }
}
