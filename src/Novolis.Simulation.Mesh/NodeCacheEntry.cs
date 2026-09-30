using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Per-node cache membership: when received + local retention priority snapshot.</summary>
public sealed record NodeCacheEntry(
  long ReceivedHour,
  int LocalPriority,
  int? LocalTtlHours = null);
