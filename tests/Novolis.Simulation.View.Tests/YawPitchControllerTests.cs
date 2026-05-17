using System.Numerics;
using Novolis.Simulation.View;
using TUnit.Core;

namespace Novolis.Simulation.View.Tests;

public class YawPitchControllerTests
{
    [Test]
    public async Task GetForwardXZ_AtYawZero_PointsAlongPositiveZ()
    {
        var cam = new YawPitchController { Yaw = 0f };
        await Assert.That(cam.GetForwardXZ()).IsEqualTo(new Vector3(0f, 0f, 1f));
    }

    [Test]
    public async Task GetForwardXZ_AtYawHalfPi_PointsAlongPositiveX()
    {
        var cam = new YawPitchController { Yaw = MathF.PI / 2f };
        await Assert.That(cam.GetForwardXZ().X).IsEqualTo(1f).Within(1e-4f);
        await Assert.That(cam.GetForwardXZ().Z).IsEqualTo(0f).Within(1e-4f);
    }

    [Test]
    public async Task GetRightXZ_IsPerpendicularToForwardXZ()
    {
        var cam = new YawPitchController { Yaw = 0.7f };
        var dot = Vector3.Dot(cam.GetForwardXZ(), cam.GetRightXZ());
        await Assert.That(dot).IsEqualTo(0f).Within(1e-5f);
    }

    [Test]
    public async Task AddLookDelta_ClampsPitch()
    {
        var cam = new YawPitchController();
        cam.AddLookDelta(0f, 10f);
        await Assert.That(cam.Pitch).IsLessThan(MathF.PI / 2f);
        await Assert.That(cam.Pitch).IsGreaterThan(MathF.PI / 2f - 0.1f);
    }

    [Test]
    public async Task GetLookDirection_WithPitch_HasYComponent()
    {
        var cam = new YawPitchController { Yaw = 0f, Pitch = 0.3f };
        await Assert.That(cam.GetLookDirection().Y).IsNotEqualTo(0f);
    }
}
