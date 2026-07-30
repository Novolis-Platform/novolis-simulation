using System.Numerics;
using Novolis.Math.Geometry;
using Novolis.Physics.Collision.Simple;
using Novolis.Physics.Joints;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Import;
using Novolis.Simulation.Humanoid.Physics;
using Novolis.Simulation.Humanoid.Skinning;

namespace Novolis.Simulation.Humanoid.Tests;

public class HumanoidPipelineTests
{
    [Test]
    public async Task RagdollBridge_BuildStandingFromBind_HasElevenSpheres()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var spheres = new List<SphereState>();
        var joints = new List<DistanceJoint>();
        var swings = new List<SwingLimit>();
        var hinges = new List<HingeLimit>();

        HumanoidRagdollBridge.BuildStandingFromBind(bind, spheres, joints, swings, hinges);

        await Assert.That(spheres.Count).IsEqualTo(11);
        await Assert.That(Vector3.Distance(spheres[0].Position, bind[HumanoidBone.Hips])).IsLessThan(0.05f);
    }

    [Test]
    public async Task RagdollBridge_RoundTrip_PreservesHips()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
        var spheres = new List<SphereState>();
        var joints = new List<DistanceJoint>();
        var swings = new List<SwingLimit>();
        var hinges = new List<HingeLimit>();
        HumanoidRagdollBridge.BuildStandingFromBind(bind, spheres, joints, swings, hinges);
        HumanoidRagdollBridge.ApplyWorldPoseToSpheres(world, spheres);
        var back = HumanoidRagdollBridge.WorldPoseFromSpheres(spheres);

        await Assert.That(Vector3.Distance(back.Position(HumanoidBone.Hips), world.Position(HumanoidBone.Hips)))
            .IsLessThan(1e-4f);
    }

    [Test]
    public async Task BvhImporter_ParsesMinimalClip()
    {
        const string bvh = """
            HIERARCHY
            ROOT Hips
            {
              OFFSET 0 0 0
              CHANNELS 6 Xposition Yposition Zposition Zrotation Xrotation Yrotation
              JOINT LeftUpLeg
              {
                OFFSET 0 0 0
                CHANNELS 3 Zrotation Xrotation Yrotation
                End Site
                {
                  OFFSET 0 -10 0
                }
              }
            }
            MOTION
            Frames: 2
            Frame Time: 0.033333
            0 100 0 0 0 0 0 0 0
            10 100 0 0 0 0 10 0 0
            """;

        var clip = BvhHumanoidImporter.Import(bvh);
        await Assert.That(clip.Keys.Count).IsEqualTo(2);
        await Assert.That(clip.Keys[0].RootTranslation).IsNotNull();
        await Assert.That(clip.Keys[0].RootTranslation!.Value.Y).IsEqualTo(1f).Within(0.01f);
    }

    [Test]
    public async Task GltfImporter_ReadsNamedNodes()
    {
        const string gltf = """
            {
              "nodes": [
                { "name": "Hips", "translation": [0, 0.92, 0], "rotation": [0, 0, 0, 1] },
                { "name": "mixamorig:Head", "rotation": [0, 0, 0.1, 0.995] }
              ]
            }
            """;

        var clip = GltfHumanoidImporter.ImportBindPose(gltf);
        await Assert.That(clip.Keys.Count).IsEqualTo(1);
        await Assert.That(clip.Keys[0].LocalRotations.ContainsKey(HumanoidBone.Head)).IsTrue();
    }

    [Test]
    public async Task CpuSkinDeformer_SingleBone_MovesVertex()
    {
        var bind = HumanoidBindPose.CreateDefaultTPose();
        var mesh = new TriangleMesh(
            [bind[HumanoidBone.LeftHand], bind[HumanoidBone.LeftHand] + Vector3.UnitX * 0.01f, bind[HumanoidBone.LeftHand] + Vector3.UnitY * 0.01f],
            [0, 1, 2]);
        var weights = new[]
        {
            new[] { new VertexBoneWeight(HumanoidBone.LeftHand, 1f) },
            new[] { new VertexBoneWeight(HumanoidBone.LeftHand, 1f) },
            new[] { new VertexBoneWeight(HumanoidBone.LeftHand, 1f) },
        };
        var skin = new SkinnedHumanoidMesh(mesh, weights, SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind));
        var pose = HumanoidPose.FromBind(bind, bind[HumanoidBone.Hips] + new Vector3(1f, 0f, 0f));
        var world = HumanoidPoseSolver.SolveWorld(bind, pose);
        // Force hand world position shift for the test by rewriting world pose.
        world.Set(HumanoidBone.LeftHand, bind[HumanoidBone.LeftHand] + new Vector3(2f, 0f, 0f), Quaternion.Identity);

        var dest = new Vector3[3];
        CpuSkinDeformer.Deform(skin, world, dest);
        await Assert.That(dest[0].X).IsEqualTo(bind[HumanoidBone.LeftHand].X + 2f).Within(0.05f);
    }
}
