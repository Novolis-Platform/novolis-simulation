using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>
/// Feed subscriptions (Atom/RSS-style). <see cref="MeshFeedId.Emergency"/> is always effective
/// even if missing from <see cref="FeedIds"/>.
/// </summary>
public sealed record MeshSubscriptionBook(
  MeshIdentityId Owner,
  ImmutableHashSet<string> FeedIds);
