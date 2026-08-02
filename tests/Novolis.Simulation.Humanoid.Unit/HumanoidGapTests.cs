using System.Numerics;
using Novolis.Math.Geometry;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Skinning;

namespace Novolis.Simulation.Humanoid.Tests;

public sealed class HumanoidGapTests
{
    [Test]
    public async Task RagdollMap_MapsLimbsToSpheres()
    {
        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.Hips))
            .IsEqualTo(HumanoidRagdollMap.RagdollHip);
        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.LeftHand))
            .IsEqualTo(HumanoidRagdollMap.RagdollLeftHand);
        await Assert.That(HumanoidRagdollMap.ToRagdollSphere(HumanoidBone.LeftToeBase))
            .IsEqualTo(HumanoidRagdollMap.RagdollLeftFoot);
    }

    [Test]
    public async Task NearestBoneSkinner_BindsSimpleMesh()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var chest = bind[HumanoidBone.Spine2];
        var mesh = new TriangleMesh(
            [chest, chest + new Vector3(0.02f, 0, 0), chest + new Vector3(0, 0.02f, 0)],
            [0, 1, 2]);
        var skinned = HumanoidNearestBoneSkinner.Bind(mesh, bind, influences: 2);

        await Assert.That(skinned.BindMesh.VertexCount).IsEqualTo(mesh.VertexCount);
        await Assert.That(skinned.VertexWeights[0][0].Weight).IsGreaterThan(0f);
    }

    [Test]
    public async Task AdaptiveBody_CreateFromBind_HasTriangles()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
        var centers = new Vector3[HumanoidAdaptiveBody.SphereCount];
        for (var i = 0; i < centers.Length; i++)
            centers[i] = bind[(HumanoidBone)System.Math.Min(i, (int)HumanoidBone.Count - 1)];

        var body = HumanoidAdaptiveBody.CreateFromRagdollBind(centers);
        await Assert.That(body.VertexCount).IsGreaterThan(20);
        await Assert.That(body.TriangleCount).IsGreaterThan(20);
    }
}
