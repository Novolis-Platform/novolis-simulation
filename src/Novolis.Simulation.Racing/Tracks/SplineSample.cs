namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public readonly record struct SplineSample(Vector2 Position, Vector2 Tangent, Vector2 Normal);
