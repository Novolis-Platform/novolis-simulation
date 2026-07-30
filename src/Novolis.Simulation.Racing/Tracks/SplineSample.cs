namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Normal.</summary>
/// <summary>Tangent.</summary>
/// <summary>Position.</summary>
/// <summary>SplineSample operation.</summary>
/// <summary>Represents SplineSample.</summary>
public readonly record struct SplineSample(Vector3 Position, Vector3 Tangent, Vector3 Normal);
