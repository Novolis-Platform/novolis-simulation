using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Durable mesh relay node (one per star system in the campaign seed).</summary>
public sealed record MeshNode(
  MeshNodeId Id,
  string SystemId,
  string Name,
  int PulseBandwidthPerHour);
