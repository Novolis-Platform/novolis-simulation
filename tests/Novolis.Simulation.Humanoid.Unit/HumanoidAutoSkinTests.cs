using System.Numerics;
using Novolis.Math.Geometry;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Skinning;

namespace Novolis.Simulation.Humanoid.Tests;

public class HumanoidAutoSkinTests
{
    [Test]
    public async Task NearestBoneSkinner_HandVertex_FollowsHand()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var hand = bind[HumanoidBone.RightHand];
        var mesh = new TriangleMesh(
            [hand, hand + new Vector3(0.02f, 0, 0), hand + new Vector3(0, 0.02f, 0)],
            [0, 1, 2]);

        var skin = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 2);
        await Assert.That(skin.VertexWeights[0][0].Bone).IsEqualTo(HumanoidBone.RightHand);

        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        world.Set(HumanoidBone.RightHand, hand + new Vector3(0.4f, 0.1f, 0f), Quaternion.Identity);

        var dest = new Vector3[3];
        CpuSkinDeformer.Deform(skin, world, dest);
        await Assert.That(Vector3.Distance(dest[0], hand + new Vector3(0.4f, 0.1f, 0f))).IsLessThan(0.08f);
    }

    [Test]
    public async Task MeshAligner_FitsHeightAndFeet()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.8f);
        // Mesh 2 m tall, offset in XZ and buried below ground.
        var mesh = new EditableMesh(
            [
                new Vector3(1f, -0.5f, 2f),
                new Vector3(1.2f, -0.5f, 2f),
                new Vector3(1f, 1.5f, 2f),
            ],
            [0, 1, 2]);

        HumanoidMeshAligner.FitToBindPose(mesh, bind);
        var minY = float.MaxValue;
        var maxY = float.MinValue;
        for (var i = 0; i < mesh.VertexCount; i++)
        {
            minY = MathF.Min(minY, mesh.Vertices[i].Y);
            maxY = MathF.Max(maxY, mesh.Vertices[i].Y);
        }

        await Assert.That(minY).IsEqualTo(0f).Within(1e-3f);
        await Assert.That(maxY - minY).IsEqualTo(1.8f).Within(1e-2f);
    }

    [Test]
    public async Task TryMapBoneName_MixamoPrefix()
    {
        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName("mixamorig:Hips"))
            .IsEqualTo(HumanoidBone.Hips);
        await Assert.That(HumanoidNearestBoneSkinner.TryMapBoneName("RightHand"))
            .IsEqualTo(HumanoidBone.RightHand);
    }

    [Test]
    public async Task DeformTranslations_RestPose_LeavesMeshNearBind()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var hand = bind[HumanoidBone.RightHand];
        var mesh = new TriangleMesh(
            [hand, hand + new Vector3(0.02f, 0, 0), hand + new Vector3(0, 0.02f, 0)],
            [0, 1, 2]);
        var skin = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 2);
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var dest = new Vector3[3];
        CpuSkinDeformer.DeformTranslations(skin, bind, world, dest);
        await Assert.That(Vector3.Distance(dest[0], hand)).IsLessThan(0.02f);
    }
}
