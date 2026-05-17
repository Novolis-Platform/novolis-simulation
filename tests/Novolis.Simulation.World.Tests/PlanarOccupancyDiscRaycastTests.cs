using System.Numerics;
using TUnit.Core;

namespace Novolis.Simulation.World.Tests;

public sealed class PlanarOccupancyDiscRaycastTests
{
    [Test]
    public async Task TryRaycastDisc_HitsCenteredTarget()
    {
        var origin = new Vector3(0, 1, 0);
        var dir = Vector3.UnitZ;
        var target = new Vector3(0, 0, 10);
        var hit = PlanarOccupancy.TryRaycastDisc(origin, dir, target, maxRange: 50, hitRadius: 1f, out var info);
        await Assert.That(hit).IsTrue();
        await Assert.That(info.DistanceAlongRay).IsEqualTo(10f).Within(0.01f);
    }

    [Test]
    public async Task TryRaycastDisc_MissesWideTarget()
    {
        var origin = new Vector3(0, 0, 0);
        var dir = Vector3.UnitZ;
        var target = new Vector3(5, 0, 10);
        var hit = PlanarOccupancy.TryRaycastDisc(origin, dir, target, maxRange: 50, hitRadius: 0.5f, out _);
        await Assert.That(hit).IsFalse();
    }
}
