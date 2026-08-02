using System.Numerics;
using Novolis.Simulation.Humanoid;

namespace Novolis.Simulation.Humanoid.Tests;

public class HumanoidFullBodyIkTests
{
    [Test]
    public async Task Apply_DualHands_MovesBothEffectors_PreservesHips()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var hips = world.Position(HumanoidBone.Hips);

        static Vector3 InReach(HumanoidBindPose b, HumanoidWorldPose w, HumanoidBone root, HumanoidBone mid, HumanoidBone end, Vector3 dir)
        {
            var u = Vector3.Distance(b[root], b[mid]);
            var l = Vector3.Distance(b[mid], b[end]);
            return w.Position(root) + Vector3.Normalize(dir) * (u + l) * 0.65f;
        }

        var leftTarget = InReach(bind, world, HumanoidBone.LeftArm, HumanoidBone.LeftForeArm, HumanoidBone.LeftHand, new Vector3(-0.2f, 0.3f, 0.5f));
        var rightTarget = InReach(bind, world, HumanoidBone.RightArm, HumanoidBone.RightForeArm, HumanoidBone.RightHand, new Vector3(0.2f, 0.3f, 0.5f));

        var targets = HumanoidFullBodyIkTargets.WithDefaults();
        targets.LeftHand = leftTarget;
        targets.RightHand = rightTarget;
        HumanoidFullBodyIk.Apply(world, bind, targets);

        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.Hips), hips)).IsLessThan(1e-4f);
        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.LeftHand), leftTarget)).IsLessThan(1e-3f);
        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.RightHand), rightTarget)).IsLessThan(1e-3f);
    }

    [Test]
    public async Task Apply_FootTarget_MatchesTwoBoneReach()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var u = Vector3.Distance(bind[HumanoidBone.LeftUpLeg], bind[HumanoidBone.LeftLeg]);
        var l = Vector3.Distance(bind[HumanoidBone.LeftLeg], bind[HumanoidBone.LeftFoot]);
        var root = world.Position(HumanoidBone.LeftUpLeg);
        var footTarget = root + Vector3.Normalize(new Vector3(0.05f, -0.9f, 0.15f)) * (u + l) * 0.7f;

        var targets = HumanoidFullBodyIkTargets.WithDefaults();
        targets.LeftFoot = footTarget;
        HumanoidFullBodyIk.Apply(world, bind, targets);

        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.LeftFoot), footTarget)).IsLessThan(1e-3f);
        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.LeftUpLeg), world.Position(HumanoidBone.LeftLeg)))
            .IsEqualTo(u).Within(1e-3f);
    }
}
