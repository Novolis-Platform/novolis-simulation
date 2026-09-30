using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>
/// Mailbox for a person, household, firm, ship, or thing — parked at a star-system node.
/// Identity-addressed packets <b>push</b> here only while co-located with a node that holds them.
/// <see cref="LinkedToNode"/> is false while the hull is in FTL (corridor Underway) — no pull/push.
/// </summary>
public sealed record MeshMailbox(
  MeshIdentityId Owner,
  MeshIdentityKind Kind,
  MeshNodeId LocationNodeId,
  ImmutableHashSet<string> PushedPacketKeys,
  bool LinkedToNode = true);
