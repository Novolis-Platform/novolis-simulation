namespace Novolis.Simulation.Replay;

/// <summary>
/// Collects per-actor plans before a simultaneous commit phase (WEGO-style), without owning simulation rules.
/// </summary>
/// <typeparam name="TPlan">Opaque plan type defined by the host game.</typeparam>
public sealed class SimultaneousPlanBuffer<TPlan>
    where TPlan : notnull
{
    private readonly Dictionary<int, TPlan> _plans = [];

    /// <summary>Clears all pending plans.</summary>
    public void Clear() => _plans.Clear();

    /// <summary>Registers or replaces a plan for an actor.</summary>
    /// <param name="actorId">Stable actor identifier.</param>
    /// <param name="plan">Plan submitted for the upcoming commit.</param>
    public void Submit(int actorId, TPlan plan) => _plans[actorId] = plan;

    /// <summary>Returns pending plans in ascending actor id order.</summary>
    public IReadOnlyList<KeyValuePair<int, TPlan>> PendingPlans() =>
        _plans.OrderBy(static kv => kv.Key).ToArray();
}
