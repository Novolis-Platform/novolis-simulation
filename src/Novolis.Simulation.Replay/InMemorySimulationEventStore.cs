namespace Novolis.Simulation.Replay;

/// <summary>Append-only in-memory event store for tests and spikes.</summary>
public sealed class InMemorySimulationEventStore<TEvent> : ISimulationEventStore<TEvent>
{
    private readonly List<TEvent> _events = [];

    /// <inheritdoc />
    public ValueTask AppendAsync(TEvent evt, CancellationToken cancellationToken = default)
    {
        _events.Add(evt);
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<TEvent> ReadAllAsync([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        foreach (var evt in _events)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return evt;
            await Task.Yield();
        }
    }
}
