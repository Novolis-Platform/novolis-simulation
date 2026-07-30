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

    [Test]
    public async Task AdjustDistance_ClampsToMax()
    {
        var cam = new OrbitCameraRig { Distance = 9f, MinDistance = 1f, MaxDistance = 10f };
        cam.AdjustDistance(5f);
        await Assert.That(cam.Distance).IsEqualTo(10f);
    }

    [Test]
    public async Task BuildEyePosition_IsOffsetFromTarget()
    {
        var cam = new OrbitCameraRig
        {
            Target = Vector3.Zero,
            Distance = 2f,
            Yaw = 0f,
            Pitch = 0f,
        };
        var eye = cam.BuildEyePosition();
        await Assert.That(eye.Length()).IsGreaterThan(1.5f);
    }
}
