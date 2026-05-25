using System.Numerics;
using Novolis.Simulation.View;

namespace Novolis.Simulation.Unit.View;

public class ViewPoseRenderingBridgeTests
{
    [Test]
    public async Task Observer_frame_has_unit_forward()
    {
        var pose = new ViewPose(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, 60f);
        var frame = ViewPoseRenderingBridge.ToObserverFrame(pose, 16f / 9f);
        await Assert.That(frame.Forward.Length()).IsCloseTo(1f, 0.001f);
        await Assert.That(frame.AspectRatio).IsEqualTo(16f / 9f);
    }
}
