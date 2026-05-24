using System.Numerics;
using Novolis.Math.Geometry;
using Novolis.Simulation.World.Builders;
using TUnit.Core;

namespace Novolis.Simulation.World.Builders.Tests;

public sealed class OccupancyEnclosedRoomMeshBuilderTests
{
    [Test]
    public async Task FromWallGrid_FloorBlocksDownwardRay()
    {
        Span<byte> cells = [0, 0, 0, 0];
        var world = OccupancyEnclosedRoomMeshBuilder.FromWallGrid(2, 2, cells, cellSize: 1f, wallHeight: 4f);

        var ray = new Ray3(new Vector3(1f, 2f, 1f), Vector3.Normalize(new Vector3(0f, -1f, 0f)));
        var hit = world.Raycast(in ray, maxDistance: 10.0, out var info);

        await Assert.That(hit).IsTrue();
        await Assert.That(info.Distance).IsGreaterThan(0);
    }
}
