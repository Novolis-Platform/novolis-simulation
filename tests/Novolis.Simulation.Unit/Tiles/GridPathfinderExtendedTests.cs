using Novolis.Simulation.Tiles;

namespace Novolis.Simulation.Unit.Tiles;

public sealed class GridPathfinderExtendedTests
{
    [Test]
    public async Task FindPath_AllowsDiagonal_WhenCornersClear()
    {
        var walls = new WallEdgeMap(5, 5);
        var blocked = new bool[5, 5];
        var path = GridPathfinder.FindPath(walls, (0, 0), (2, 2), blocked, allowDiagonal: true);
        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Count).IsLessThan(5);
    }

    [Test]
    public async Task FindPath_BlocksDiagonal_WhenOrthoEdgeBlocked()
    {
        var walls = new WallEdgeMap(3, 3);
        walls.SetV(1, 0, WallEdge.Solid);
        var path = GridPathfinder.FindPath(walls, (0, 0), (2, 2), allowDiagonal: true);
        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Any(p => p == (1, 1))).IsFalse();
    }

    [Test]
    public async Task FindPath_ReturnsNull_WhenGoalBlocked()
    {
        var walls = new WallEdgeMap(3, 3);
        var blocked = new bool[3, 3];
        blocked[2, 2] = true;
        var path = GridPathfinder.FindPath(walls, (0, 0), (2, 2), blocked);
        await Assert.That(path).IsNull();
    }

    [Test]
    public async Task FindPath_ReturnsNull_WhenOutOfBounds()
    {
        var walls = new WallEdgeMap(2, 2);
        await Assert.That(GridPathfinder.FindPath(walls, (-1, 0), (1, 1))).IsNull();
        await Assert.That(GridPathfinder.FindPath(walls, (0, 0), (5, 5))).IsNull();
    }

    [Test]
    public async Task FindPath_PrefersShorterOrthoRoute_OnOpenGrid()
    {
        var walls = new WallEdgeMap(4, 4);
        var path = GridPathfinder.FindPath(walls, (0, 0), (3, 0));
        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Count).IsEqualTo(4);
        await Assert.That(path[0]).IsEqualTo((0, 0));
        await Assert.That(path[^1]).IsEqualTo((3, 0));
    }

    [Test]
    public async Task WalkabilityMask_FromBlockedCells_MarksPredicateCells()
    {
        var grid = WalkabilityMask.FromBlockedCells(4, 4, (x, z) => x == 2 && z == 2);
        await Assert.That(grid[2u, 0u, 2u]).IsEqualTo((byte)1);
        await Assert.That(grid[0u, 0u, 0u]).IsEqualTo((byte)0);
    }
}
