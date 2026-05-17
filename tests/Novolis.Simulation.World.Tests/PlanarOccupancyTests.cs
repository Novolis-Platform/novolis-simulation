using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Simulation.World;
using TUnit.Core;

namespace Novolis.Simulation.World.Tests;

public class PlanarOccupancyTests
{
    [Test]
    public async Task TryMove_OpenFloor_AppliesFullDelta()
    {
        var map = new DenseGrid<byte>(4, 4);
        var start = new Vector3(1.5f, 0f, 1.5f);
        var end = PlanarOccupancy.TryMove(map, start, new Vector3(0.3f, 0f, 0.2f), 0.2f);
        await Assert.That(end.X).IsEqualTo(1.8f).Within(1e-4f);
        await Assert.That(end.Z).IsEqualTo(1.7f).Within(1e-4f);
    }

    [Test]
    public async Task TryMove_IntoWall_BlocksAxis()
    {
        var map = new DenseGrid<byte>(4, 4);
        map.Set(new GridIndex(2, 1), 1);

        var start = new Vector3(1.5f, 0f, 1.5f);
        var end = PlanarOccupancy.TryMove(map, start, new Vector3(1f, 0f, 0f), 0.2f);
        await Assert.That(end.X).IsLessThan(2f);
        await Assert.That(end.Z).IsEqualTo(1.5f).Within(1e-4f);
    }

    [Test]
    public async Task OverlapsWall_OutsideMap_ReturnsTrue()
    {
        var map = new DenseGrid<byte>(3, 3);
        await Assert.That(PlanarOccupancy.OverlapsWall(map, new Vector3(-1f, 0f, 1f), 0.2f)).IsTrue();
    }

    [Test]
    public async Task OverlapsWall_InsideOpenCell_ReturnsFalse()
    {
        var map = new DenseGrid<byte>(5, 5);
        map.Set(new GridIndex(2, 2), 1);
        await Assert.That(PlanarOccupancy.OverlapsWall(map, new Vector3(1.5f, 0f, 1.5f), 0.25f)).IsFalse();
    }

    [Test]
    public async Task HasLineOfSight_OpenLine_ReturnsTrue()
    {
        var map = new DenseGrid<byte>(5, 5);
        await Assert.That(PlanarOccupancy.HasLineOfSight(map, new Vector3(0.5f, 0f, 0.5f), new Vector3(3.5f, 0f, 0.5f))).IsTrue();
    }

    [Test]
    public async Task HasLineOfSight_WallBetween_ReturnsFalse()
    {
        var map = new DenseGrid<byte>(5, 5);
        map.Set(new GridIndex(2, 0), 1);
        map.Set(new GridIndex(2, 1), 1);
        map.Set(new GridIndex(2, 2), 1);
        map.Set(new GridIndex(2, 3), 1);
        map.Set(new GridIndex(2, 4), 1);
        await Assert.That(PlanarOccupancy.HasLineOfSight(map, new Vector3(0.5f, 0f, 2.5f), new Vector3(4.5f, 0f, 2.5f))).IsFalse();
    }

    [Test]
    public async Task HasLineOfSight_AroundCorner_ReturnsFalse()
    {
        var map = new DenseGrid<byte>(5, 5);
        map.Set(new GridIndex(2, 1), 1);
        map.Set(new GridIndex(2, 2), 1);
        map.Set(new GridIndex(1, 2), 1);
        await Assert.That(PlanarOccupancy.HasLineOfSight(map, new Vector3(0.5f, 0f, 0.5f), new Vector3(3.5f, 0f, 3.5f))).IsFalse();
    }

    [Test]
    public async Task HasLineOfSight_TwoCellWideCorridor_WithClearance_ReturnsTrue()
    {
        var map = new DenseGrid<byte>(7, 5);
        map.Set(new GridIndex(0, 0), 1);
        map.Set(new GridIndex(0, 1), 1);
        map.Set(new GridIndex(0, 2), 1);
        map.Set(new GridIndex(0, 3), 1);
        map.Set(new GridIndex(0, 4), 1);
        map.Set(new GridIndex(6, 0), 1);
        map.Set(new GridIndex(6, 1), 1);
        map.Set(new GridIndex(6, 2), 1);
        map.Set(new GridIndex(6, 3), 1);
        map.Set(new GridIndex(6, 4), 1);

        await Assert.That(
                PlanarOccupancy.HasLineOfSight(
                    map,
                    new Vector3(1.5f, 0f, 2.5f),
                    new Vector3(4.5f, 0f, 2.5f),
                    clearanceRadius: 0.4f))
            .IsTrue();
    }

    [Test]
    public async Task HasLineOfSight_SingleCellGap_WithClearance_ReturnsFalse()
    {
        var map = new DenseGrid<byte>(5, 5);
        map.Set(new GridIndex(2, 0), 1);
        map.Set(new GridIndex(2, 1), 1);
        map.Set(new GridIndex(2, 2), 1);
        map.Set(new GridIndex(2, 3), 1);
        map.Set(new GridIndex(2, 4), 1);

        await Assert.That(
                PlanarOccupancy.HasLineOfSight(
                    map,
                    new Vector3(0.5f, 0f, 2.5f),
                    new Vector3(4.5f, 0f, 2.5f),
                    clearanceRadius: 0.45f))
            .IsFalse();
    }

    [Test]
    public async Task PushOutOfWalls_FromInsideWall_MovesToOpenFloor()
    {
        var map = new DenseGrid<byte>(5, 5);
        map.Set(new GridIndex(2, 2), 1);

        var pushed = PlanarOccupancy.PushOutOfWalls(map, new Vector3(2.5f, 0f, 2.5f), 0.3f);
        await Assert.That(PlanarOccupancy.OverlapsWall(map, pushed, 0.3f)).IsFalse();
    }

    [Test]
    public async Task TryRaycastWall_HitsWallBeforeOpenEnd()
    {
        var map = new DenseGrid<byte>(5, 5);
        map.Set(new GridIndex(2, 0), 1);
        map.Set(new GridIndex(2, 1), 1);
        map.Set(new GridIndex(2, 2), 1);
        map.Set(new GridIndex(2, 3), 1);
        map.Set(new GridIndex(2, 4), 1);

        var hit = PlanarOccupancy.TryRaycastWall(
            map,
            new Vector3(0.5f, 0f, 2.5f),
            new Vector3(1f, 0f, 0f),
            maxDistance: 10f,
            cellSize: 1f,
            out var hitDistance);

        await Assert.That(hit).IsTrue();
        await Assert.That(hitDistance).IsLessThan(2.5f);
    }

    [Test]
    public async Task TryRaycastWall_OpenCorridor_ReturnsFalse()
    {
        var map = new DenseGrid<byte>(5, 5);
        var hit = PlanarOccupancy.TryRaycastWall(
            map,
            new Vector3(0.5f, 0f, 0.5f),
            new Vector3(1f, 0f, 0f),
            maxDistance: 3f,
            cellSize: 1f,
            out _);

        await Assert.That(hit).IsFalse();
    }
}
