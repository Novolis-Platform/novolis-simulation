using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Bandwidth-gated launch waiting at a node.</summary>
public sealed record PendingLaunch(
  PacketId PacketId,
  MeshNodeId From,
  MeshNodeId To,
  ImmutableArray<MeshNodeId> RemainingPathAfterArrival,
  bool IsFloodHop,
  int Priority);
