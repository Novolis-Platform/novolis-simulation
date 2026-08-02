using System.Numerics;
using Novolis.Simulation.Humanoid;

namespace Novolis.Simulation.Humanoid.Tests;

public class FabrikChainTests
{
    [Test]
    public async Task Solve_ReachableTarget_EndsNearTarget()
    {
        Span<Vector3> positions =
        [
            Vector3.Zero,
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, 2f, 0f),
        ];
        Span<float> lengths = [1f, 1f];
        var target = new Vector3(1.2f, 0.8f, 0f);

        var reached = FabrikChain.Solve(positions, lengths, target, pinRoot: true, maxIterations: 24, tolerance: 1e-3f);
        var tip = positions[^1];
        var root = positions[0];
        var seg0 = Vector3.Distance(positions[0], positions[1]);
        var seg1 = Vector3.Distance(positions[1], positions[2]);

        await Assert.That(reached).IsTrue();
        await Assert.That(Vector3.Distance(tip, target)).IsLessThan(1e-3f);
        await Assert.That(root).IsEqualTo(Vector3.Zero);
        await Assert.That(seg0).IsEqualTo(1f).Within(1e-3f);
        await Assert.That(seg1).IsEqualTo(1f).Within(1e-3f);
    }

    [Test]
    public async Task Solve_Unreachable_StretchesTowardTarget()
    {
        Span<Vector3> positions =
        [
            Vector3.Zero,
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, 2f, 0f),
        ];
        Span<float> lengths = [1f, 1f];
        var target = new Vector3(10f, 0f, 0f);

        var reached = FabrikChain.Solve(positions, lengths, target, pinRoot: true);
        var tip = positions[^1];
        var root = positions[0];
        var tipDist = Vector3.Distance(tip, Vector3.Zero);
        var tipDirX = Vector3.Normalize(tip).X;

        await Assert.That(reached).IsFalse();
        await Assert.That(root).IsEqualTo(Vector3.Zero);
        await Assert.That(tipDist).IsEqualTo(2f).Within(1e-3f);
        await Assert.That(tipDirX).IsGreaterThan(0.99f);
    }
}

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
