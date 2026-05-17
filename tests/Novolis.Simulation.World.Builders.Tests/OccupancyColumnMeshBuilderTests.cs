using Novolis.Physics.Numerics;
using Novolis.Simulation.World.Builders;
using TUnit.Core;

namespace Novolis.Simulation.World.Builders.Tests;

public sealed class OccupancyColumnMeshBuilderTests
{
    [Test]
    public async Task FromWallGrid_BuildsMeshWithTriangles()
    {
        Span<byte> cells = [0, 1, 0, 1];
        var world = OccupancyColumnMeshBuilder.FromWallGrid(2, 2, cells);

        var ray = new Ray3d(new Vector3d(1.5, 1.0, -1.0), new Vector3d(0, 0, 1).Normalized());
        var hit = world.Raycast(in ray, maxDistance: 10.0, out var info);

        await Assert.That(hit).IsTrue();
        await Assert.That(info.Distance).IsGreaterThan(0);
    }
}
