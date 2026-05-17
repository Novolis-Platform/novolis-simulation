using System.Numerics;
using Novolis.Physics.Collision.Simple;

namespace Novolis.Simulation.World.Builders;

public sealed class HeightfieldBuildResult
{
    public required BvhStaticWorld Collision { get; init; }

    public required Vector3[] DrawVertices { get; init; }

    public int DrawCells { get; init; }
}
