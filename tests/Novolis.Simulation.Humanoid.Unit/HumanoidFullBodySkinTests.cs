using System.Numerics;
using Novolis.Math.Geometry;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Skinning;

namespace Novolis.Simulation.Humanoid.Tests;

public class HumanoidFullBodySkinTests
{
    [Test]
    public async Task NearestBoneSkinner_StickFigure_DrillMovesUpperMoreThanFeet()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var mesh = BuildStickFigure(bind);
        var skin = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 3);

        var covered = new bool[(int)HumanoidBone.Count];
        foreach (var w in skin.VertexWeights)
        {
            if (w.Length > 0)
                covered[(int)w[0].Bone] = true;
        }

        await Assert.That(covered.Count(static c => c)).IsGreaterThanOrEqualTo(8);

        var order = new HumanoidAnimationClip("order") { Loop = false };
        order.AddKey(new HumanoidKeyframe
        {
            TimeSeconds = 0f,
            RootTranslation = bind[HumanoidBone.Hips],
            LocalRotations =
            {
                [HumanoidBone.RightArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.15f),
                [HumanoidBone.RightForeArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -0.3f),
            },
        });
        var present = new HumanoidAnimationClip("present") { Loop = false };
        present.AddKey(new HumanoidKeyframe
        {
            TimeSeconds = 0f,
            RootTranslation = bind[HumanoidBone.Hips] + new Vector3(0.02f, 0f, 0.04f),
            LocalRotations =
            {
                [HumanoidBone.RightArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.9f) *
                                          Quaternion.CreateFromAxisAngle(Vector3.UnitY, -0.6f),
                [HumanoidBone.RightForeArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -1.3f),
                [HumanoidBone.LeftArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.8f),
                [HumanoidBone.LeftForeArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -1.2f),
                [HumanoidBone.Spine] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.15f),
                [HumanoidBone.Spine2] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.12f),
                [HumanoidBone.RightUpLeg] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 0.12f),
                [HumanoidBone.RightLeg] = Quaternion.CreateFromAxisAngle(Vector3.UnitX, -0.18f),
            },
        });

        var pose = new HumanoidPose();
        order.Sample(0f, pose, bind);
        var worldA = HumanoidPoseSolver.SolveWorld(bind, pose);
        var bufA = new Vector3[mesh.VertexCount];
        CpuSkinDeformer.Deform(skin, worldA, bufA);

        present.Sample(0f, pose, bind);
        var worldB = HumanoidPoseSolver.SolveWorld(bind, pose);
        var bufB = new Vector3[mesh.VertexCount];
        CpuSkinDeformer.Deform(skin, worldB, bufB);

        var hipsY = bind[HumanoidBone.Hips].Y;
        float upperMax = 0f, footMean = 0f;
        var footN = 0;
        for (var i = 0; i < mesh.VertexCount; i++)
        {
            var d = Vector3.Distance(bufA[i], bufB[i]);
            if (mesh.Vertices[i].Y > hipsY + 0.1f)
                upperMax = MathF.Max(upperMax, d);
            else if (mesh.Vertices[i].Y < hipsY - 0.15f)
            {
                footMean += d;
                footN++;
            }
        }

        footMean = footN == 0 ? 0f : footMean / footN;
        await Assert.That(upperMax).IsGreaterThan(0.05f);
        await Assert.That(upperMax).IsGreaterThan(footMean);
    }

    [Test]
    public async Task NearestBoneSkinner_EveryVertexHasNormalizedWeights()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var mesh = BuildStickFigure(bind);
        var skin = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 4);
        for (var v = 0; v < skin.VertexWeights.Count; v++)
        {
            var sum = skin.VertexWeights[v].Sum(w => w.Weight);
            await Assert.That(sum).IsEqualTo(1f).Within(1e-3f);
        }
    }

    [Test]
    public async Task NearestBoneSkinner_LimbVertsPreferLimbBonesOverHips()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var hand = bind[HumanoidBone.RightHand];
        var forearm = bind[HumanoidBone.RightForeArm];
        var mid = (hand + forearm) * 0.5f;
        var mesh = new TriangleMesh(
            [mid, mid + new Vector3(0.01f, 0, 0), mid + new Vector3(0, 0.01f, 0)],
            [0, 1, 2]);
        var skin = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 3);
        var primary = skin.VertexWeights[0][0].Bone;
        await Assert.That(
            primary is HumanoidBone.RightHand or HumanoidBone.RightForeArm or HumanoidBone.RightArm)
            .IsTrue();
    }

    static TriangleMesh BuildStickFigure(HumanoidBindPose bind)
    {
        var verts = new List<Vector3>();
        var inds = new List<int>();
        void Capsule(Vector3 a, Vector3 b)
        {
            var i0 = verts.Count;
            verts.Add(a);
            verts.Add(b);
            verts.Add((a + b) * 0.5f + Vector3.UnitX * 0.02f);
            inds.Add(i0);
            inds.Add(i0 + 1);
            inds.Add(i0 + 2);
        }

        Capsule(bind[HumanoidBone.Hips], bind[HumanoidBone.Spine2]);
        Capsule(bind[HumanoidBone.Spine2], bind[HumanoidBone.Head]);
        Capsule(bind[HumanoidBone.Spine2], bind[HumanoidBone.LeftHand]);
        Capsule(bind[HumanoidBone.Spine2], bind[HumanoidBone.RightHand]);
        Capsule(bind[HumanoidBone.Hips], bind[HumanoidBone.LeftFoot]);
        Capsule(bind[HumanoidBone.Hips], bind[HumanoidBone.RightFoot]);
        Capsule(bind[HumanoidBone.LeftArm], bind[HumanoidBone.LeftHand]);
        Capsule(bind[HumanoidBone.RightArm], bind[HumanoidBone.RightHand]);
        Capsule(bind[HumanoidBone.LeftUpLeg], bind[HumanoidBone.LeftFoot]);
        Capsule(bind[HumanoidBone.RightUpLeg], bind[HumanoidBone.RightFoot]);
        return new TriangleMesh(verts, inds);
    }
}
