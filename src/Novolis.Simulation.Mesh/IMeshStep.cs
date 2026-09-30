namespace Novolis.Simulation.Mesh;

/// <summary>One ordered transformation of <see cref="MeshState"/>.</summary>
public interface IMeshStep
{
  /// <summary>string Name { get; }.</summary>
  string Name { get; }
  /// <summary>MeshState Execute(MeshState current);.</summary>
  MeshState Execute(MeshState current);
}
