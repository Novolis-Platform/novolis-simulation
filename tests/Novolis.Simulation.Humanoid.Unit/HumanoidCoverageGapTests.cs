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

    [Test]
    public async Task NearestBoneSkinner_Bind_RejectsInvalidInfluencesAndEmptyMesh()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var mesh = new Novolis.Math.Geometry.TriangleMesh(
            [bind[HumanoidBone.Head], bind[HumanoidBone.Head] + Vector3.UnitX, bind[HumanoidBone.Head] + Vector3.UnitY],
            [0, 1, 2]);
        var empty = new Novolis.Math.Geometry.TriangleMesh([], []);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
        {
            HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 0);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
        {
            HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 9);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            HumanoidNearestBoneSkinner.Bind(empty, bind);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task NearestBoneSkinner_Bind_NearLimbAndCoincidentShaft()
    {
        var basePose = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var positions = new Vector3[(int)HumanoidBone.Count];
        var present = new bool[(int)HumanoidBone.Count];
        for (var i = 0; i < positions.Length; i++)
        {
            positions[i] = basePose[(HumanoidBone)i];
            present[i] = true;
        }

        // Collapse LeftForeArm onto LeftArm so DistanceToBoneSq hits lenSq < 1e-10.
        positions[(int)HumanoidBone.LeftForeArm] = positions[(int)HumanoidBone.LeftArm];
        var bind = HumanoidBindPose.FromWorldPositions(positions, present, 1.72f);

        var arm = bind[HumanoidBone.LeftArm];
        var mesh = new Novolis.Math.Geometry.TriangleMesh(
            [
                arm,
                arm + new Vector3(0.01f, 0, 0),
                arm + new Vector3(0, 0.01f, 0),
                bind[HumanoidBone.Hips],
                bind[HumanoidBone.Hips] + Vector3.UnitX * 0.02f,
                bind[HumanoidBone.Hips] + Vector3.UnitZ * 0.02f,
            ],
            [0, 1, 2, 3, 4, 5]);

        var skinned = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 2);
        await Assert.That(skinned.VertexWeights.Count).IsEqualTo(6);
        await Assert.That(skinned.VertexWeights[0].Length).IsGreaterThan(0);
    }

    [Test]
    public async Task NearestBoneSkinner_TryMapBoneName_CoversAliasesAndPrefixes()
    {
        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName(null)).IsNull();
        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName("   ")).IsNull();
        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName("mixamorig:Hips")).IsEqualTo(HumanoidBone.Hips);
        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName("mixamorig_LeftHand")).IsEqualTo(HumanoidBone.LeftHand);
        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName("LeftArm")).IsEqualTo(HumanoidBone.LeftArm);

        (string Name, HumanoidBone Bone)[] aliases =
        [
            ("hip", HumanoidBone.Hips),
            ("pelvis", HumanoidBone.Hips),
            ("chest", HumanoidBone.Spine2),
            ("upperchest", HumanoidBone.Spine2),
            ("l_upleg", HumanoidBone.LeftUpLeg),
            ("leftthigh", HumanoidBone.LeftUpLeg),
            ("r_upleg", HumanoidBone.RightUpLeg),
            ("rightthigh", HumanoidBone.RightUpLeg),
            ("l_leg", HumanoidBone.LeftLeg),
            ("leftcalf", HumanoidBone.LeftLeg),
            ("r_leg", HumanoidBone.RightLeg),
            ("rightcalf", HumanoidBone.RightLeg),
            ("l_foot", HumanoidBone.LeftFoot),
            ("r_foot", HumanoidBone.RightFoot),
            ("l_arm", HumanoidBone.LeftArm),
            ("leftupperarm", HumanoidBone.LeftArm),
            ("r_arm", HumanoidBone.RightArm),
            ("rightupperarm", HumanoidBone.RightArm),
            ("l_forearm", HumanoidBone.LeftForeArm),
            ("leftlowerarm", HumanoidBone.LeftForeArm),
            ("r_forearm", HumanoidBone.RightForeArm),
            ("rightlowerarm", HumanoidBone.RightForeArm),
            ("l_hand", HumanoidBone.LeftHand),
            ("r_hand", HumanoidBone.RightHand),
        ];

        foreach (var (name, bone) in aliases)
            await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName(name)).IsEqualTo(bone);

        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName("totally_unknown")).IsNull();
    }

    [Test]
    public async Task NearestBoneSkinner_TryBindNamedWeights_MapsAndRejectsPoorMapping()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.7f);
        var mesh = new Novolis.Math.Geometry.TriangleMesh(
            [bind[HumanoidBone.Hips], bind[HumanoidBone.Head], bind[HumanoidBone.LeftHand]],
            [0, 1, 2]);

        var good = new NamedBoneWeight[][]
        {
            [new NamedBoneWeight("mixamorig:Hips", 1f)],
            [new NamedBoneWeight("Head", 0.6f), new NamedBoneWeight("Neck", 0.4f)],
            [new NamedBoneWeight("l_hand", 1f)],
        };
        var skinned = HumanoidNearestBoneSkinner.TryBindNamedWeights(mesh, good, bind);
        await Assert.That(skinned).IsNotNull();
        await Assert.That(skinned!.VertexWeights[2][0].Bone).IsEqualTo(HumanoidBone.LeftHand);

        var withNullRow = new NamedBoneWeight[]?[]
        {
            null,
            [new NamedBoneWeight("unknown", 1f), new NamedBoneWeight("also_bad", 1f)],
            [new NamedBoneWeight("LeftArm", 0f)],
        };
        // 1 mapped of 3 influences → <50% → null
        var fallback = HumanoidNearestBoneSkinner.TryBindNamedWeights(mesh, withNullRow!, bind);
        await Assert.That(fallback).IsNull();

        var mostlyMapped = new NamedBoneWeight[][]
        {
            [new NamedBoneWeight("Hips", 1f)],
            [new NamedBoneWeight("Head", 1f)],
            [new NamedBoneWeight("nope", 1f)],
        };
        // 2 of 3 mapped → ≥50% → succeeds with hips fallback on last vert
        var partial = HumanoidNearestBoneSkinner.TryBindNamedWeights(mesh, mostlyMapped, bind);
        await Assert.That(partial).IsNotNull();
        await Assert.That(partial!.VertexWeights[2][0].Bone).IsEqualTo(HumanoidBone.Hips);

        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            HumanoidNearestBoneSkinner.TryBindNamedWeights(mesh, [good[0]], bind);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
        {
            HumanoidNearestBoneSkinner.TryBindNamedWeights(null!, good, bind);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task CpuSkinDeformer_RejectsShortDestination_AndHandlesZeroWeights()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.75f);
        var mesh = new Novolis.Math.Geometry.TriangleMesh(
            [bind[HumanoidBone.Hips], bind[HumanoidBone.Hips] + Vector3.UnitX, bind[HumanoidBone.Hips] + Vector3.UnitY],
            [0, 1, 2]);
        var skin = new SkinnedHumanoidMesh(
            mesh,
            [
                [new VertexBoneWeight(HumanoidBone.Hips, 1f)],
                [],
                [new VertexBoneWeight(HumanoidBone.Head, 0f)],
            ],
            SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind));
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));

        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            var shortDest = new Vector3[1];
            CpuSkinDeformer.Deform(skin, world, shortDest);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            var shortDest = new Vector3[2];
            CpuSkinDeformer.DeformTranslations(skin, bind, world, shortDest);
            return Task.CompletedTask;
        });

        var dest = new Vector3[3];
        CpuSkinDeformer.Deform(skin, world, dest);
        CpuSkinDeformer.DeformTranslations(skin, bind, world, dest);
        await Assert.That(dest[1]).IsEqualTo(mesh.Vertices[1]); // zero-weight → bind position

        var deformed = CpuSkinDeformer.DeformToMesh(skin, world);
        await Assert.That(deformed.VertexCount).IsEqualTo(3);
        await Assert.That(deformed.Indices.Length).IsEqualTo(3);
    }

    [Test]
    public async Task SkinnedHumanoidMesh_Ctor_ValidatesWeightsAndInverseBinds()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.7f);
        var mesh = new Novolis.Math.Geometry.TriangleMesh(
            [Vector3.Zero, Vector3.UnitX, Vector3.UnitY],
            [0, 1, 2]);
        var inv = SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind);

        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            _ = new SkinnedHumanoidMesh(mesh, [[]], inv);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            _ = new SkinnedHumanoidMesh(
                mesh,
                [[], [], []],
                new Matrix4x4[1]);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task MeshAligner_HandlesEmptyFlatAndTriangleMeshOverload()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.8f);
        var empty = new Novolis.Math.Geometry.EditableMesh([], []);
        HumanoidMeshAligner.FitToBindPose(empty, bind);
        await Assert.That(empty.VertexCount).IsEqualTo(0);

        var flat = new Novolis.Math.Geometry.EditableMesh(
            [new Vector3(0, 1, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 1)],
            [0, 1, 2]);
        HumanoidMeshAligner.FitToBindPose(flat, bind);
        await Assert.That(flat.Vertices[0].Y).IsEqualTo(1f);

        var tri = new Novolis.Math.Geometry.TriangleMesh(
            [new Vector3(0, 0, 0), new Vector3(1, 2, 0), new Vector3(0, 2, 1)],
            [0, 1, 2]);
        var fitted = HumanoidMeshAligner.FitToBindPose(tri, bind);
        await Assert.That(fitted.VertexCount).IsEqualTo(3);
        var minY = fitted.Vertices[0].Y;
        for (var i = 1; i < fitted.Vertices.Length; i++)
            minY = MathF.Min(minY, fitted.Vertices[i].Y);
        await Assert.That(minY).IsGreaterThanOrEqualTo(-1e-4f);
    }

    [Test]
    public async Task AdaptiveBody_RejectsShortSphereBuffers()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            HumanoidAdaptiveBody.CreateFromRagdollBind(new Vector3[3]);
            return Task.CompletedTask;
        });

        var handles = new Vector3[4];
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            HumanoidAdaptiveBody.CopySphereCenters(new Vector3[2], handles);
            return Task.CompletedTask;
        });
    }

    [Test]
    public async Task FullBodyIk_FeetHeadAndDefaultPoles()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var u = Vector3.Distance(bind[HumanoidBone.RightUpLeg], bind[HumanoidBone.RightLeg]);
        var l = Vector3.Distance(bind[HumanoidBone.RightLeg], bind[HumanoidBone.RightFoot]);
        var footTarget = world.Position(HumanoidBone.RightUpLeg)
            + Vector3.Normalize(new Vector3(-0.05f, -0.9f, 0.1f)) * (u + l) * 0.7f;
        var headTarget = world.Position(HumanoidBone.Head) + new Vector3(0.05f, 0.08f, 0.12f);

        var targets = new HumanoidFullBodyIkTargets
        {
            RightFoot = footTarget,
            Head = headTarget,
            // zero poles → Apply fills UnitZ defaults
        };
        HumanoidFullBodyIk.Apply(world, bind, targets);
        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.RightFoot), footTarget)).IsLessThan(0.05f);
        await Assert.That(Vector3.Distance(world.Position(HumanoidBone.Head), headTarget)).IsLessThan(0.15f);
    }

    [Test]
    public async Task BindPose_ValidatesHeightAndBoneLength()
    {
        await Assert.That(HumanoidBindPose.CreateDefaultTPose(1.8f).WorldPositions.Length)
            .IsEqualTo((int)HumanoidBone.Count);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
        {
            HumanoidBindPose.CreateDefaultTPose(0.1f);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            HumanoidBindPose.FromWorldPositions([], [], 1.7f);
            return Task.CompletedTask;
        });

        var bind = HumanoidBindPose.CreateDefaultTPose(1.7f);
        await Assert.That(bind.BoneLength(HumanoidBone.Hips)).IsEqualTo(0f);
        await Assert.That(bind.BoneLength(HumanoidBone.Head)).IsGreaterThan(0f);
    }

    [Test]
    public async Task Fabrik_RejectsAndUnreachableCoincident()
    {
        var shortChain = new Vector3[1];
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            FabrikChain.Solve(shortChain, ReadOnlySpan<float>.Empty, Vector3.UnitX);
            return Task.CompletedTask;
        });

        var positions = new Vector3[] { Vector3.Zero, Vector3.UnitY, new Vector3(0, 2, 0) };
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            FabrikChain.Solve(positions, new float[] { 1f }, Vector3.UnitX);
            return Task.CompletedTask;
        });
        var lengths = new float[] { 1f, 1f };
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
        {
            FabrikChain.Solve(positions, lengths, Vector3.UnitX, maxIterations: 0);
            return Task.CompletedTask;
        });
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
        {
            FabrikChain.Solve(positions, new float[] { -1f, 1f }, Vector3.UnitX);
            return Task.CompletedTask;
        });

        await Assert.That(FabrikChain.Solve(positions, lengths, new Vector3(0, 10, 0))).IsFalse();
        await Assert.That(FabrikChain.ConstrainDistance(Vector3.Zero, Vector3.Zero, 2f).Y).IsEqualTo(2f);
    }

    [Test]
    public async Task AnimationPoseSolver_AndTwoBoneEdges()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.75f);
        var pose = HumanoidPose.FromBind(bind);
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            pose.CopyLocalRotationsTo(new Quaternion[1]);
            return Task.CompletedTask;
        });
        var full = new Quaternion[(int)HumanoidBone.Count];
        pose.CopyLocalRotationsTo(full);

        var positions = HumanoidPoseSolver.SolvePositions(bind, pose);
        await Assert.That(positions.Length).IsEqualTo((int)HumanoidBone.Count);

        var clip = new HumanoidAnimationClip("idle");
        clip.Sample(0f, pose, bind); // empty keys
        await Assert.That(pose.RootTranslation).IsEqualTo(bind[HumanoidBone.Hips]);

        clip.AddKey(new HumanoidKeyframe
        {
            TimeSeconds = 0f,
            RootTranslation = bind[HumanoidBone.Hips],
            LocalRotations = { [HumanoidBone.Spine] = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.1f) },
        });
        clip.Sample(0f, pose, bind); // single key
        clip.Loop = false;
        clip.AddKey(new HumanoidKeyframe
        {
            TimeSeconds = 1f,
            LocalRotations = { [HumanoidBone.Spine] = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 0.4f) },
        });
        clip.Sample(2f, pose, bind); // clamp past end
        clip.Loop = true;
        clip.Sample(-0.25f, pose, bind); // negative wrap
        clip.Sample(0.5f, pose); // interpolate without bind root

        // Near-zero key span + key without root uses bind hips in Apply.
        clip.AddKey(new HumanoidKeyframe
        {
            TimeSeconds = 1f + 1e-6f,
            LocalRotations = { [HumanoidBone.Neck] = Quaternion.Identity },
        });
        clip.Sample(1f + 5e-7f, pose, bind);

        await Assert.That(HumanoidBoneNames.Canonical(HumanoidBone.Count)).IsEqualTo("Count");
        await Assert.That(HumanoidBoneNames.TryResolve("", out _)).IsFalse();
        await Assert.That(HumanoidBoneNames.TryResolve("Armature|mixamorig:Head", out var resolvedHead)).IsTrue();
        await Assert.That(resolvedHead).IsEqualTo(HumanoidBone.Head);

        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.Count)).IsEqualTo(-1);
        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            HumanoidChainIk.Apply(
                HumanoidPoseSolver.SolveWorld(bind, pose),
                bind,
                new[] { HumanoidBone.Head },
                Vector3.Zero);
            return Task.CompletedTask;
        });

        var mid = TwoBoneIk.SolveMid(Vector3.Zero, Vector3.UnitY, 1f, 1f, Vector3.UnitY); // parallel pole
        await Assert.That(mid.Length()).IsGreaterThan(0f);
        mid = TwoBoneIk.SolveMid(Vector3.Zero, Vector3.UnitX, 1f, 1f, Vector3.UnitX); // second axis fallback
        await Assert.That(TwoBoneIk.EnforceBendSide(Vector3.Zero, Vector3.UnitY, Vector3.Zero, Vector3.UnitZ))
            .IsEqualTo(Vector3.UnitY);
        await Assert.That(TwoBoneIk.EnforceBendSide(Vector3.Zero, Vector3.UnitY, Vector3.UnitZ, Vector3.UnitZ))
            .IsEqualTo(Vector3.UnitY); // preferred cross ~0
        await Assert.That(TwoBoneIk.ClampReach(Vector3.Zero, Vector3.Zero, 1f, 1f).Y).IsLessThan(0f);
        await Assert.That(TwoBoneIk.FromToRotation(Vector3.Zero, Vector3.UnitX)).IsEqualTo(Quaternion.Identity);
        var q = TwoBoneIk.FromToRotation(Vector3.UnitX, Vector3.UnitX);
        await Assert.That(q).IsEqualTo(Quaternion.Identity);
        q = TwoBoneIk.FromToRotation(Vector3.UnitX, -Vector3.UnitX);
        await Assert.That(Vector3.Dot(Vector3.Transform(Vector3.UnitX, q), -Vector3.UnitX)).IsGreaterThan(0.99f);

        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        TwoBoneIk.ApplyLimb(
            world, bind,
            HumanoidBone.LeftArm, HumanoidBone.LeftForeArm, HumanoidBone.LeftHand,
            world.Position(HumanoidBone.LeftArm) + new Vector3(0.2f, -0.1f, 0.3f),
            Vector3.Distance(bind[HumanoidBone.LeftArm], bind[HumanoidBone.LeftForeArm]),
            Vector3.Distance(bind[HumanoidBone.LeftForeArm], bind[HumanoidBone.LeftHand]),
            Vector3.UnitZ);
    }
}
