using System.Numerics;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Skinning;

namespace Novolis.Simulation.Humanoid.Tests;

public sealed class HumanoidCoverageGapTests
{
    [Test]
    public async Task BoneNames_CanonicalCoversAllStandardBones()
    {
        for (var i = 0; i < (int)HumanoidBone.Count; i++)
        {
            var bone = (HumanoidBone)i;
            var name = HumanoidBoneNames.Canonical(bone);
            await Assert.That(string.IsNullOrWhiteSpace(name)).IsFalse();
        }
    }

    [Test]
    public async Task BoneNames_TryResolveMixamoAndPrefixedAliases()
    {
        await Assert.That(HumanoidBoneNames.TryResolve("mixamorig:LeftHand", out var hand)).IsTrue();
        await Assert.That(hand).IsEqualTo(HumanoidBone.LeftHand);
        await Assert.That(HumanoidBoneNames.TryResolve("Armature|RightFoot", out var foot)).IsTrue();
        await Assert.That(foot).IsEqualTo(HumanoidBone.RightFoot);
        await Assert.That(HumanoidBoneNames.TryResolve("UnknownJoint", out _)).IsFalse();
    }

    [Test]
    public async Task RagdollMap_MapsEveryBoneOrReturnsNegativeOne()
    {
        for (var i = 0; i < (int)HumanoidBone.Count; i++)
        {
            var bone = (HumanoidBone)i;
            var sphere = HumanoidRagdollMap.ToRagdollSphere(bone);
            await Assert.That(sphere).IsGreaterThanOrEqualTo(-1);
            await Assert.That(sphere).IsLessThan(11);
        }

        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.Spine1))
            .IsEqualTo(HumanoidRagdollMap.RagdollChest);
        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.RightForeArm))
            .IsEqualTo(HumanoidRagdollMap.RagdollRightHand);
    }

    [Test]
    public async Task NearestBoneSkinner_MapsCommonImportNames()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var mesh = new Novolis.Math.Geometry.TriangleMesh(
            [bind[HumanoidBone.Head], bind[HumanoidBone.Head] + Vector3.UnitX, bind[HumanoidBone.Head] + Vector3.UnitY],
            [0, 1, 2]);
        var skinned = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 4);
        await Assert.That(skinned.VertexWeights.Count).IsEqualTo(mesh.VertexCount);
    }

    [Test]
    public async Task TwoBoneIk_LegacyOverload_UpdatesMidAndEnd()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var root = HumanoidBone.LeftArm;
        var mid = HumanoidBone.LeftForeArm;
        var end = HumanoidBone.LeftHand;
        var u = Vector3.Distance(bind[root], bind[mid]);
        var l = Vector3.Distance(bind[mid], bind[end]);
        var target = world.Position(root) + new Vector3(0.2f, -0.1f, 0.3f);

        TwoBoneIk.ApplyLimb(world, root, mid, end, target, u, l, Vector3.UnitZ);
        await Assert.That(Vector3.Distance(world.Position(end), target)).IsLessThan(0.05f);
    }

    [Test]
    public async Task TwoBoneIk_SolveMid_HandlesCoincidentRootAndTarget()
    {
        var mid = TwoBoneIk.SolveMid(Vector3.Zero, Vector3.Zero, 1f, 1f, Vector3.UnitZ);
        await Assert.That(mid.Y).IsLessThan(0f);
    }

    [Test]
    public async Task TwoBoneIk_FromToRotation_OppositeVectors()
    {
        var q = TwoBoneIk.FromToRotation(Vector3.UnitY, -Vector3.UnitY);
        var rotated = Vector3.Transform(Vector3.UnitY, q);
        await Assert.That(Vector3.Dot(rotated, -Vector3.UnitY)).IsGreaterThan(0.99f);
    }
}
