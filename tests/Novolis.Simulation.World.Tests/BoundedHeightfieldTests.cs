using System.Numerics;
using Novolis.Physics.Abstractions;
using Novolis.Simulation.World;
using TUnit.Core;

namespace Novolis.Simulation.World.Tests;

public sealed class BoundedHeightfieldTests
{
    [Test]
    public async Task ProjectOntoSurface_ClampsToGround()
    {
        var field = new BoundedHeightfield(new FlatSampler(), 100f);
        var p = field.ProjectOntoSurface(new Vector3(120, 500, -5));
        await Assert.That(p.X).IsEqualTo(100f).Within(0.001f);
        await Assert.That(p.Z).IsEqualTo(0f).Within(0.001f);
        await Assert.That(p.Y).IsEqualTo(0.05f).Within(0.001f);
    }

    private sealed class FlatSampler : IHeightSampler
    {
        public float SampleHeight(float x, float z) => 0f;
    }
}
