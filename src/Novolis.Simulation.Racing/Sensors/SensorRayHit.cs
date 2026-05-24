namespace Novolis.Simulation.Racing.Sensors;

using System.Numerics;

/// <summary>HitWall.</summary>
/// <summary>Distance.</summary>
/// <summary>Direction.</summary>
/// <summary>Origin.</summary>
/// <summary>Name.</summary>
/// <summary>SensorRayHit operation.</summary>
/// <summary>Represents SensorRayHit.</summary>
public sealed record SensorRayHit(string Name, Vector2 Origin, Vector2 Direction, double Distance, bool HitWall);
