using System.Numerics;
using Novolis.Physics.Collision.Simple;

namespace Novolis.Simulation.World.Builders;

/// <summary>Represents HeightfieldBuildResult.</summary>
public sealed class HeightfieldBuildResult
{
    /// <summary>Collision.</summary>
    public required BvhStaticWorld Collision { get; init; }

    /// <summary>DrawVertices.</summary>
    public required Vector3[] DrawVertices { get; init; }

    /// <summary>DrawCells.</summary>
    public int DrawCells { get; init; }
}
