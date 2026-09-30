using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Disposable pulse/bulk carrier in transit.</summary>
public sealed record InFlightDrone(
  DroneId Id,
  PacketId PacketId,
  MeshNodeId From,
  MeshNodeId To,
  int RemainingHours,
  ImmutableArray<MeshNodeId> RemainingPathAfterArrival,
  bool IsFloodHop,
  int Priority);
