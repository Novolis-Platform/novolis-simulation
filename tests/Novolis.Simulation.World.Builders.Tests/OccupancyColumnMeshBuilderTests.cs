using System.Numerics;
using Novolis.Math.Geometry;
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

        var ray = new Ray3(new Vector3(1.5f, 1f, -1f), Vector3.Normalize(new Vector3(0f, 0f, 1f)));
        var hit = world.Raycast(in ray, maxDistance: 10.0, out var info);

        await Assert.That(hit).IsTrue();
        await Assert.That(info.Distance).IsGreaterThan(0);
    }
}
