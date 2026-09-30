namespace Novolis.Simulation.Mesh;

/// <summary>Published packet id.</summary>
public readonly record struct PacketId(Guid Value)
{
  /// <summary>New.</summary>
  public static PacketId New() => new(Guid.NewGuid());
  /// <summary>From.</summary>
  public static PacketId From(Guid value) => new(value);
}
