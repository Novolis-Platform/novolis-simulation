namespace Novolis.Simulation.Mesh;

/// <summary>In-flight disposable drone instance.</summary>
public readonly record struct DroneId(Guid Value)
{
  /// <summary>New.</summary>
  public static DroneId New() => new(Guid.NewGuid());
}
