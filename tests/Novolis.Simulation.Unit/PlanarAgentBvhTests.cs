using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Math.Geometry;
using Novolis.Physics.Collision.Simple;
using Novolis.Simulation.Kinematics;

namespace Novolis.Simulation.Unit;

public sealed class PlanarAgentBvhTests
{
    [Test]
    public async Task Move_WithBvh_StopsAtWallOnX()
    {
        var walls = new DenseGrid<byte>(16, 16);
        var world = BuildWallAtX(3f);
        var pos = new Vector3(1f, 0f, 1f);
        var next = PlanarAgent.Move(walls, pos, new Vector3(5f, 0f, 0f), radius: 0.25f, cellSize: 1f, world);
        await Assert.That(next.X).IsLessThan(3f);
        await Assert.That(next.X).IsGreaterThan(1f);
    }

    [Test]
    public async Task Move_WithBvh_StopsAtWallOnZ()
    {
        var walls = new DenseGrid<byte>(16, 16);
        var world = BuildWallAtZ(4f);
        var pos = new Vector3(1f, 0f, 1f);
        var next = PlanarAgent.Move(walls, pos, new Vector3(0f, 0f, 6f), radius: 0.25f, cellSize: 1f, world);
        await Assert.That(next.Z).IsLessThan(4f);
        await Assert.That(next.Z).IsGreaterThan(1f);
    }

    [Test]
    public async Task Move_WithBvh_PassesThroughOpenSpace()
    {
        var walls = new DenseGrid<byte>(16, 16);
        var world = BuildWallAtX(10f);
        var pos = new Vector3(1f, 0f, 1f);
        var next = PlanarAgent.Move(walls, pos, new Vector3(2f, 0f, 1.5f), radius: 0.2f, cellSize: 1f, world);
        await Assert.That(next.X).IsEqualTo(3f);
        await Assert.That(next.Z).IsEqualTo(2.5f);
    }

    private static BvhStaticWorld BuildWallAtX(float x)
    {
        var verts = new[]
        {
            new Vector3(x, 0f, -10f),
            new Vector3(x, 3f, -10f),
            new Vector3(x, 3f, 10f),
            new Vector3(x, 0f, 10f),
        };
        var indices = new[] { 0, 1, 2, 0, 2, 3 };
        return new BvhStaticWorld(new TriangleMesh(verts, indices));
    }

    private static BvhStaticWorld BuildWallAtZ(float z)
    {
        var verts = new[]
        {
            new Vector3(-10f, 0f, z),
            new Vector3(10f, 0f, z),
            new Vector3(10f, 3f, z),
            new Vector3(-10f, 3f, z),
        };
        var indices = new[] { 0, 1, 2, 0, 2, 3 };
        return new BvhStaticWorld(new TriangleMesh(verts, indices));
    }
}
