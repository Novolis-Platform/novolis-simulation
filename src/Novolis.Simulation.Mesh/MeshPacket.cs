using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Published signed object (signature opaque in BM).</summary>
public sealed record MeshPacket(
  PacketId Id,
  MeshTrafficLayer Layer,
  bool Sealed,
  ImmutableArray<byte> SignatureBlob,
  int Priority,
  int? GlobalTtlHours,
  int? LocalTtlHours,
  int LocalRetentionPriority,
  MeshNodeId OriginNode,
  MeshAddress Destination,
  long PublishedHour,
  string Subject = "",
  string Body = "",
  string Topic = "",
  string LogicalKey = "");
