using System.Numerics;
using Novolis.Simulation.Humanoid;

namespace Novolis.Simulation.Unit.Humanoid;

public class HumanoidStandardTests
{
    [Test]
    public async Task Hierarchy_Parents_MatchMixamoTree()
    {
        await Assert.That(HumanoidHierarchy.Parent(HumanoidBone.Hips)).IsEqualTo(-1);
        await Assert.That(HumanoidHierarchy.Parent(HumanoidBone.Spine)).IsEqualTo((int)HumanoidBone.Hips);
        await Assert.That(HumanoidHierarchy.Parent(HumanoidBone.LeftArm)).IsEqualTo((int)HumanoidBone.LeftShoulder);
        await Assert.That(HumanoidHierarchy.Parent(HumanoidBone.RightHand)).IsEqualTo((int)HumanoidBone.RightForeArm);
    }

    [Test]
    public async Task BoneNames_ResolveMixamoAliases()
    {
        await Assert.That(HumanoidBoneNames.TryResolve("mixamorig:Hips", out var hips)).IsTrue();
        await Assert.That(hips).IsEqualTo(HumanoidBone.Hips);
        await Assert.That(HumanoidBoneNames.TryResolve("LeftUpperArm", out var arm)).IsTrue();
        await Assert.That(arm).IsEqualTo(HumanoidBone.LeftArm);
        await Assert.That(HumanoidBoneNames.Canonical(HumanoidBone.Spine2)).IsEqualTo("Spine2");
    }

    [Test]
    public async Task BindPose_TPose_IsAboutHumanHeight()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.8f);
        await Assert.That(bind[HumanoidBone.Head].Y).IsGreaterThan(1.5f);
        await Assert.That(bind[HumanoidBone.Head].Y).IsLessThan(1.85f);
        await Assert.That(bind[HumanoidBone.LeftHand].X).IsLessThan(-0.5f);
        await Assert.That(bind[HumanoidBone.RightHand].X).IsGreaterThan(0.5f);
    }

    [Test]
    public async Task PoseSolver_IdentityLocals_MatchBindAtRoot()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var pose = HumanoidPose.FromBind(bind);
        var world = HumanoidPoseSolver.SolveWorld(bind, pose);

        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.Hips), bind[HumanoidBone.Hips])).IsLessThan(1e-4f);
        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.Head), bind[HumanoidBone.Head])).IsLessThan(1e-3f);
        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.LeftHand), bind[HumanoidBone.LeftHand])).IsLessThan(1e-3f);
    }

    [Test]
    public async Task PoseSolver_RootMove_TranslatesWholeBody()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var pose = HumanoidPose.FromBind(bind, new Vector3(10f, 0.92f, 5f));
        var world = HumanoidPoseSolver.SolveWorld(bind, pose);

        await Assert.That(world.Position(HumanoidBone.Hips).X).IsEqualTo(10f);
        await Assert.That(world.Position(HumanoidBone.Head).X).IsEqualTo(10f).Within(1e-3f);
    }

    [Test]
    public async Task TwoBoneIk_MidStaysWithinReach()
    {
        var root = new Vector3(0f, 1f, 0f);
        var target = new Vector3(0.35f, 1f, 0f); // |root→target| = 0.35 < 0.58
        var mid = TwoBoneIk.SolveMid(root, target, 0.3f, 0.28f, Vector3.UnitZ);

        var upper = Vector3.Distance(root, mid);
        var lower = Vector3.Distance(mid, target);
        await Assert.That(upper).IsEqualTo(0.3f).Within(0.02f);
        await Assert.That(lower).IsEqualTo(0.28f).Within(0.02f);
    }

    [Test]
    public async Task AnimationClip_SamplesBetweenKeys()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var clip = new HumanoidAnimationClip("wave")
            .AddKey(new HumanoidKeyframe
            {
                TimeSeconds = 0f,
                RootTranslation = bind[HumanoidBone.Hips],
                LocalRotations = { [HumanoidBone.LeftArm] = Quaternion.Identity },
            })
            .AddKey(new HumanoidKeyframe
            {
                TimeSeconds = 1f,
                RootTranslation = bind[HumanoidBone.Hips],
                LocalRotations =
                {
                    [HumanoidBone.LeftArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 2f),
                },
            });

        var pose = new HumanoidPose();
        clip.Sample(0.5f, pose, bind);
        var angle = 2f * MathF.Acos(System.Math.Clamp(pose[HumanoidBone.LeftArm].W, -1f, 1f));
        await Assert.That(angle).IsEqualTo(MathF.PI / 4f).Within(0.05f);
    }

    [Test]
    public async Task DebugDraw_HasExpectedSegmentCount()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var segs = HumanoidDebugDraw.BuildSegments(world);
        await Assert.That(segs.Length).IsEqualTo(21);
    }

    [Test]
    public async Task RagdollMap_HipsAndHands()
    {
        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.Hips)).IsEqualTo(HumanoidRagdollMap.RagdollHip);
        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.LeftHand)).IsEqualTo(HumanoidRagdollMap.RagdollLeftHand);
    }
}
