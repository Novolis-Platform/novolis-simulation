namespace Novolis.Simulation.Mesh;

/// <summary>Mailbox / feed owner key. Prefer <see cref="MeshIdentityIds"/> factories for naming.</summary>
public readonly record struct MeshIdentityId(string Value)
{
  /// <summary>ToString.</summary>
  public override string ToString() => Value;
  /// <summary>From.</summary>
  public static MeshIdentityId From(string value) => new(value);
}
