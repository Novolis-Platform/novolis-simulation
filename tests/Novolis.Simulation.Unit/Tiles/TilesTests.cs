using Novolis.Simulation.Tiles;

namespace Novolis.Simulation.Unit.Tiles;

public sealed class TilesTests
{
    [Test]
    public async Task FloodFill_Splits_On_Solid_Wall()
    {
        var walls = new WallEdgeMap(3, 1);
        walls.SetV(1, 0, WallEdge.Solid);
        var labels = RoomFloodFill.LabelRooms(walls);
        await Assert.That(RoomFloodFill.CountRooms(labels)).IsEqualTo(2);
        await Assert.That(labels[0, 0]).IsNotEqualTo(labels[1, 0]);
        await Assert.That(labels[1, 0]).IsEqualTo(labels[2, 0]);
    }

    [Test]
    public async Task Door_Reconnects_Rooms()
    {
        var walls = new WallEdgeMap(2, 1);
        walls.SetV(1, 0, WallEdge.OpenDoor);
        var labels = RoomFloodFill.LabelRooms(walls);
        await Assert.That(RoomFloodFill.CountRooms(labels)).IsEqualTo(1);
    }

    [Test]
    public async Task Pathfinder_Goes_Around_Wall()
    {
        var walls = new WallEdgeMap(3, 3);
        walls.SetV(1, 0, WallEdge.Solid);
        walls.SetV(1, 1, WallEdge.Solid);
        var path = GridPathfinder.FindPath(walls, (0, 0), (2, 0));
        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Count).IsGreaterThan(3);
        await Assert.That(path[0]).IsEqualTo((0, 0));
        await Assert.That(path[^1]).IsEqualTo((2, 0));
    }

    [Test]
    public async Task BuildBatch_Unions_Dirty()
    {
        var batch = new BuildBatch(10, 10);
        batch.TouchCell(1, 1);
        batch.TouchCell(4, 2);
        await Assert.That(batch.Dirty.MinX).IsEqualTo(1);
        await Assert.That(batch.Dirty.MaxX).IsEqualTo(5);
        batch.ClearDirty();
        await Assert.That(batch.Dirty.IsEmpty).IsTrue();
    }

    [Test]
    public async Task TileMap_Stores_Layers()
    {
        var map = new TileMap2D(4, 4);
        map.Set(TileLayerKind.Floor, 1, 2, 9);
        map.Set(TileLayerKind.Object, 1, 2, 3);
        await Assert.That(map.Get(TileLayerKind.Floor, 1, 2)).IsEqualTo((ushort)9);
        await Assert.That(map.Get(TileLayerKind.Object, 1, 2)).IsEqualTo((ushort)3);
        var occ = WalkabilityMask.FromObjectLayer(map);
        await Assert.That(occ[1u, 0u, 2u]).IsEqualTo((byte)1);
    }
}
