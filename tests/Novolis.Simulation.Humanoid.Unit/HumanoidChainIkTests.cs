using System.Numerics;
using Novolis.Simulation.Humanoid;

namespace Novolis.Simulation.Humanoid.Tests;

public class HumanoidChainIkTests
{
    [Test]
    public async Task Apply_SpineToHead_MovesHeadTowardTarget()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var spineRoot = world.Position(HumanoidBone.Spine);
        var target = world.Position(HumanoidBone.Head) + new Vector3(0.08f, 0f, 0.08f);

        HumanoidBone[] chain =
        [
            HumanoidBone.Spine,
            HumanoidBone.Spine1,
            HumanoidBone.Spine2,
            HumanoidBone.Neck,
            HumanoidBone.Head,
        ];

        HumanoidChainIk.Apply(world, bind, chain, target, pinRoot: true, maxIterations: 32);

        var spineAfter = world.Position(HumanoidBone.Spine);
        var headAfter = world.Position(HumanoidBone.Head);
        await Assert.That(Vector3.Distance(spineAfter, spineRoot)).IsLessThan(1e-4f);
        await Assert.That(Vector3.Distance(headAfter, target)).IsLessThan(0.02f);
    }
}
