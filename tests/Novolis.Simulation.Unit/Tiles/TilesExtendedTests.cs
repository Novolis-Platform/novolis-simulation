using Novolis.Simulation.Tiles;

namespace Novolis.Simulation.Unit.Tiles;

public sealed class TilesExtendedTests
{
    [Test]
    public async Task Pathfinder_ReturnsNull_WhenBlocked()
    {
        var walls = new WallEdgeMap(3, 3);
        walls.SetV(1, 0, WallEdge.Solid);
        walls.SetV(1, 1, WallEdge.Solid);
        walls.SetV(1, 2, WallEdge.Solid);
        var path = GridPathfinder.FindPath(walls, (0, 0), (2, 2));
        await Assert.That(path).IsNull();
    }

    [Test]
    public async Task Pathfinder_ReachesAdjacentRoom_ThroughOpenDoor()
    {
        var walls = new WallEdgeMap(2, 1);
        walls.SetV(1, 0, WallEdge.OpenDoor);
        var path = GridPathfinder.FindPath(walls, (0, 0), (1, 0));
        await Assert.That(path).IsNotNull();
        await Assert.That(path![^1]).IsEqualTo((1, 0));
    }

    [Test]
    public async Task WallEdgeMap_TracksHorizontalAndVertical()
    {
        var walls = new WallEdgeMap(4, 3);
        walls.SetH(1, 1, WallEdge.Solid);
        walls.SetV(2, 0, WallEdge.Solid);
        await Assert.That(walls.GetH(1, 1)).IsEqualTo(WallEdge.Solid);
        await Assert.That(walls.GetV(2, 0)).IsEqualTo(WallEdge.Solid);
    }

    [Test]
    public async Task BuildBatch_MergesOverlappingTouches()
    {
        var batch = new BuildBatch(20, 20);
        batch.TouchCell(2, 2);
        batch.TouchCell(3, 2);
        batch.TouchCell(10, 10);
        await Assert.That(batch.Dirty.MinX).IsEqualTo(2);
        await Assert.That(batch.Dirty.MaxX).IsEqualTo(11);
        await Assert.That(batch.Dirty.MinZ).IsEqualTo(2);
        await Assert.That(batch.Dirty.MaxZ).IsEqualTo(11);
    }

    [Test]
    public async Task WalkabilityMask_BlocksOccupiedCells()
    {
        var map = new TileMap2D(3, 3);
        map.Set(TileLayerKind.Object, 1, 2, 7);
        var mask = WalkabilityMask.FromObjectLayer(map);
        await Assert.That(mask[1u, 0u, 2u]).IsEqualTo((byte)1);
        await Assert.That(mask[0u, 0u, 0u]).IsEqualTo((byte)0);
    }
}
