using System.Numerics;
using Novolis.Simulation.View;
using TUnit.Core;

namespace Novolis.Simulation.Unit.View;

public sealed class FixedAngleMapCameraTests
{
    [Test]
    public async Task BuildViewPose_PlacesEyeOffsetFromPanTarget()
    {
        var cam = new FixedAngleMapCamera
        {
            PanTarget = new Vector3(10f, 0f, 20f),
            Distance = 26f,
        };

        var pose = cam.BuildViewPose();

        await Assert.That(pose.Target).IsEqualTo(new Vector3(10f, 0f, 20f));
        await Assert.That(Vector3.Distance(pose.Position, pose.Target)).IsGreaterThan(20f);
        await Assert.That(pose.FieldOfViewDegrees).IsEqualTo(42f);
    }

    [Test]
    public async Task AdjustDistance_ClampsToMax()
    {
        var cam = new FixedAngleMapCamera { Distance = 40f, MaxDistance = 42f };
        cam.AdjustDistance(10f);
        await Assert.That(cam.Distance).IsEqualTo(42f);
    }

    [Test]
    public async Task SnapTo_ForcesYToZero()
    {
        var cam = new FixedAngleMapCamera();
        cam.SnapTo(new Vector3(3f, 9f, 4f));
        await Assert.That(cam.PanTarget).IsEqualTo(new Vector3(3f, 0f, 4f));
    }
}
