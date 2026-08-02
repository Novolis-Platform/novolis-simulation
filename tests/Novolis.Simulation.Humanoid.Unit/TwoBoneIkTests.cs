using System.Numerics;
using Novolis.Simulation.Humanoid;

namespace Novolis.Simulation.Humanoid.Tests;

public class TwoBoneIkTests
{
    [Test]
    public async Task ApplyLimb_WithBind_UpdatesRotationsSoHandReachesTarget()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var u = Vector3.Distance(bind[HumanoidBone.RightArm], bind[HumanoidBone.RightForeArm]);
        var l = Vector3.Distance(bind[HumanoidBone.RightForeArm], bind[HumanoidBone.RightHand]);
        var root = world.Position(HumanoidBone.RightArm);
        // In-reach: halfway between rest hand and max reach along +Y/+Z.
        var target = root + Vector3.Normalize(new Vector3(0.1f, 0.2f, 0.35f)) * (u + l) * 0.7f;

        TwoBoneIk.ApplyLimb(
            world, bind,
            HumanoidBone.RightArm, HumanoidBone.RightForeArm, HumanoidBone.RightHand,
            target, u, l, Vector3.UnitZ);

        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.RightHand), target)).IsLessThan(1e-3f);
        await Assert.That(Quaternion.Dot(world.Rotation(HumanoidBone.RightArm), Quaternion.Identity))
            .IsLessThan(0.999f);
    }

    [Test]
    public async Task ApplyLimb_Overreach_DoesNotStretchBones()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var root = HumanoidBone.LeftUpLeg;
        var mid = HumanoidBone.LeftLeg;
        var end = HumanoidBone.LeftFoot;
        var u = Vector3.Distance(bind[root], bind[mid]);
        var l = Vector3.Distance(bind[mid], bind[end]);
        var rootPos = world.Position(root);
        var far = rootPos + new Vector3(0f, -(u + l + 0.5f), 0.2f);

        TwoBoneIk.ApplyLimb(world, bind, root, mid, end, far, u, l, Vector3.UnitZ);

        var midPos = world.Position(mid);
        var endPos = world.Position(end);
        await Assert.That(Vector3.Distance(rootPos, midPos)).IsEqualTo(u).Within(1e-3f);
        await Assert.That(Vector3.Distance(midPos, endPos)).IsEqualTo(l).Within(1e-3f);
        await Assert.That(Vector3.Distance(rootPos, endPos)).IsLessThan(u + l);
    }

    [Test]
    public async Task FromToRotation_IdentityWhenSame()
    {
        var q = TwoBoneIk.FromToRotation(Vector3.UnitY, Vector3.UnitY);
        await Assert.That(Quaternion.Dot(q, Quaternion.Identity)).IsGreaterThan(0.999f);
    }

    [Test]
    public async Task EnforceBendSide_FlipsInvertedMid()
    {
        var root = Vector3.Zero;
        var end = new Vector3(0f, -1f, 0f);
        var pole = Vector3.UnitZ;
        var inverted = new Vector3(0f, -0.5f, -0.3f);
        var fixedMid = TwoBoneIk.EnforceBendSide(root, inverted, end, pole);

        var preferred = Vector3.Cross(end - root, pole);
        var actual = Vector3.Cross(end - root, fixedMid - root);
        await Assert.That(Vector3.Dot(actual, preferred)).IsGreaterThan(0f);
        await Assert.That(fixedMid.Z).IsGreaterThan(0f);
    }

    [Test]
    public async Task ApplyLimb_KneeStaysOnPoleSide()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var root = HumanoidBone.LeftUpLeg;
        var mid = HumanoidBone.LeftLeg;
        var end = HumanoidBone.LeftFoot;
        var pole = Vector3.UnitZ;
        var u = Vector3.Distance(bind[root], bind[mid]);
        var l = Vector3.Distance(bind[mid], bind[end]);
        var rootPos = world.Position(root);
        var target = rootPos + Vector3.Normalize(new Vector3(0.05f, -0.85f, -0.35f)) * (u + l) * 0.75f;

        TwoBoneIk.ApplyLimb(world, bind, root, mid, end, target, u, l, pole);

        var midPos = world.Position(mid);
        var endPos = world.Position(end);
        var preferred = Vector3.Cross(endPos - rootPos, pole);
        var actual = Vector3.Cross(endPos - rootPos, midPos - rootPos);
        await Assert.That(Vector3.Dot(actual, preferred)).IsGreaterThan(-1e-4f);
        await Assert.That(Vector3.Distance(endPos, target)).IsLessThan(1e-3f);
    }
}
