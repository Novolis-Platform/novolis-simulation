using Novolis.Physics.Abstractions;
using Novolis.Simulation.World;
using Novolis.Simulation.World.Builders;
using TUnit.Core;

namespace Novolis.Simulation.World.Builders.Tests;

public sealed class HeightfieldMeshBuilderTests
{
    [Test]
    public async Task Build_ProducesCollisionAndDrawMeshes()
    {
        var result = HeightfieldMeshBuilder.Build(
            new FlatSampler(),
            new WorldExtentOptions { ExtentMeters = 10f, CollisionCells = 4, DrawCells = 2 });

        await Assert.That(result.Collision).IsNotNull();
        await Assert.That(result.DrawVertices.Length).IsEqualTo(9);
        await Assert.That(result.DrawCells).IsEqualTo(2);
    }

    private sealed class FlatSampler : IHeightSampler
    {
        public float SampleHeight(float x, float z) => 0f;
    }
}
