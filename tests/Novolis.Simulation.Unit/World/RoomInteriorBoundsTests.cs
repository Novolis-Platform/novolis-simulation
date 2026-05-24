using Novolis.Simulation.World;
using TUnit.Core;

namespace Novolis.Simulation.World.Tests;

public sealed class RoomInteriorBoundsTests
{
    [Test]
    public async Task ForOccupancyGrid_InsetsBySphereRadius()
    {
        var bounds = RoomInteriorBounds.ForOccupancyGrid(12, 12, cellSize: 1f, wallHeight: 4f, sphereRadius: 0.22f);

        await Assert.That(bounds.MinY).IsEqualTo(0.22f);
        await Assert.That(bounds.MaxY).IsEqualTo(4f - 0.22f);
        await Assert.That(bounds.MinX).IsGreaterThan(1f);
    }
}
