using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Directed hop with separate pulse vs bulk travel times.</summary>
public sealed record MeshEdge(
  MeshNodeId From,
  MeshNodeId To,
  int PulseTravelHours,
  int BulkTravelHours,
  double DistanceLy);
