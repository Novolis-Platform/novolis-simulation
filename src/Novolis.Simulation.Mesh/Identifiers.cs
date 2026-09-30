namespace Novolis.Simulation.Mesh;

/// <summary>Stable mesh node key (usually Astro system id).</summary>
public readonly record struct MeshNodeId(string Value)
{
  /// <summary>ToString.</summary>
  public override string ToString() => Value;
  /// <summary>From.</summary>
  public static MeshNodeId From(string value) => new(value);
}
