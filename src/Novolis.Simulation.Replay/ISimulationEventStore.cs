namespace Novolis.Simulation.Replay;

/// <summary>
/// Optional platform hook for append-only simulation events (SCR-style journals).
/// Product-specific event types stay in application assemblies; use only when a second consumer exists.
/// </summary>
public interface ISimulationEventStore<TEvent>
{
    /// <summary>Appends an event to the store.</summary>
    ValueTask AppendAsync(TEvent evt, CancellationToken cancellationToken = default);

    /// <summary>Reads all events in append order.</summary>
    IAsyncEnumerable<TEvent> ReadAllAsync(CancellationToken cancellationToken = default);
}
