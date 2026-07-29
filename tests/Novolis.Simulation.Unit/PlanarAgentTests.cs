using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Simulation.Kinematics;

namespace Novolis.Simulation.Unit;

public sealed class PlanarAgentTests
{
    [Test]
    public async Task Move_ZeroDelta_ReturnsSamePosition()
    {
        var walls = new DenseGrid<byte>(4, 4);
        var pos = new Vector3(1f, 0f, 1f);
        var next = PlanarAgent.Move(walls, pos, Vector3.Zero, radius: 0.2f, cellSize: 1f);
        await Assert.That(next).IsEqualTo(pos);
    }

    [Test]
    public async Task Move_WithoutStaticWorld_AdvancesOnEmptyGrid()
    {
        var walls = new DenseGrid<byte>(8, 8);
        var pos = new Vector3(2f, 0f, 2f);
        var next = PlanarAgent.Move(walls, pos, new Vector3(0.5f, 0f, 0f), radius: 0.2f, cellSize: 1f);
        await Assert.That(next.X).IsGreaterThan(pos.X);
    }
}
