namespace Novolis.Simulation.Mesh;

/// <summary>Traffic layer — pulse is sprint drones; bulk is freight-class; feed is public channel cargo.</summary>
public enum MeshTrafficLayer
{
  /// <summary>Pulse.</summary>
  Pulse = 0,
  /// <summary>Bulk.</summary>
  Bulk = 1,
  /// <summary>Feed.</summary>
  Feed = 2,
}
