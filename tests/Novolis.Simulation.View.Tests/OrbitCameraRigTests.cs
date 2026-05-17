using System.Numerics;
using Novolis.Simulation.View;
using TUnit.Core;

namespace Novolis.Simulation.View.Tests;

public sealed class OrbitCameraRigTests
{
    [Test]
    public async Task BuildViewPose_SmoothsTarget()
    {
        var rig = new OrbitCameraRig { Target = Vector3.Zero, Distance = 100f };
        rig.SnapTarget(Vector3.Zero);
        _ = rig.BuildViewPose(0.016f);
        rig.Target = new Vector3(100, 0, 0);
        var pose = rig.BuildViewPose(0.016f);
        await Assert.That(pose.Target.X).IsLessThan(100f);
        await Assert.That(pose.Target.X).IsGreaterThan(0f);
    }
}
