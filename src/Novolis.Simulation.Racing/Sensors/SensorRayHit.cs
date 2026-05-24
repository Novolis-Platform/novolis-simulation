namespace Novolis.Simulation.Racing.Sensors;

using System.Numerics;

public sealed record SensorRayHit(string Name, Vector2 Origin, Vector2 Direction, double Distance, bool HitWall);
