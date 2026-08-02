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
    public async Task BvhImporter_RestBind_HasPelvisWidth()
    {
        // Prefer shipped CMU clip when the dogfood tree is checked out beside simulation.
        var shipped = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..",
            "novolis-dogfooding", "apps", "avalonia", "CharacterLab", "assets", "mocap", "02_01.bvh"));
        if (!File.Exists(shipped))
        {
            // Minimal hierarchy: hip sockets 20 units apart → ~0.2 m after height normalize.
            const string bvh = """
                HIERARCHY
                ROOT Hips
                {
                  OFFSET 0 0 0
                  CHANNELS 6 Xposition Yposition Zposition Zrotation Yrotation Xrotation
                  JOINT LeftUpLeg
                  {
                    OFFSET 10 -2 0
                    CHANNELS 3 Zrotation Yrotation Xrotation
                    JOINT LeftLeg
                    {
                      OFFSET 0 -40 0
                      CHANNELS 3 Zrotation Yrotation Xrotation
                      JOINT LeftFoot
                      {
                        OFFSET 0 -40 0
                        CHANNELS 3 Zrotation Yrotation Xrotation
                        End Site
                        {
                          OFFSET 0 -5 0
                        }
                      }
                    }
                  }
                  JOINT RightUpLeg
                  {
                    OFFSET -10 -2 0
                    CHANNELS 3 Zrotation Yrotation Xrotation
                    JOINT RightLeg
                    {
                      OFFSET 0 -40 0
                      CHANNELS 3 Zrotation Yrotation Xrotation
                      JOINT RightFoot
                      {
                        OFFSET 0 -40 0
                        CHANNELS 3 Zrotation Yrotation Xrotation
                        End Site
                        {
                          OFFSET 0 -5 0
                        }
                      }
                    }
                  }
                  JOINT LowerBack
                  {
                    OFFSET 0 20 0
                    CHANNELS 3 Zrotation Yrotation Xrotation
                    End Site
                    {
                      OFFSET 0 40 0
                    }
                  }
                }
                MOTION
                Frames: 1
                Frame Time: 0.033333
                0 100 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0
                """;
            var (_, mini) = BvhHumanoidImporter.ImportWithBind(bvh, 0.01f, null, 1.72f);
            var w = Vector3.Distance(mini[HumanoidBone.LeftUpLeg], mini[HumanoidBone.RightUpLeg]);
            await Assert.That(w).IsGreaterThan(0.12f);
            return;
        }

        var (_, bind) = BvhHumanoidImporter.ImportFileWithBind(shipped, 0.01f, BvhHumanoidImporter.RenameCmuJoint, 1.72f);
        var hipWidth = Vector3.Distance(bind[HumanoidBone.LeftUpLeg], bind[HumanoidBone.RightUpLeg]);
        await Assert.That(hipWidth).IsGreaterThan(0.18f);
        await Assert.That(bind[HumanoidBone.LeftFoot].Y).IsLessThan(0.15f);
        await Assert.That(bind[HumanoidBone.Hips].Y).IsGreaterThan(0.5f);
    }

    [Test]
    public async Task BvhImporter_FoldsCmuHipJointIntoUpLeg()
    {
        // LHipJoint Z=90° then LeftUpLeg identity → LeftUpLeg must carry the 90° fold.
        const string bvh = """
            HIERARCHY
            ROOT Hips
            {
              OFFSET 0 0 0
              CHANNELS 6 Xposition Yposition Zposition Zrotation Yrotation Xrotation
              JOINT LHipJoint
              {
                OFFSET 0 0 0
                CHANNELS 3 Zrotation Yrotation Xrotation
                JOINT LeftUpLeg
                {
                  OFFSET 1 0 0
                  CHANNELS 3 Zrotation Yrotation Xrotation
                  End Site
                  {
                    OFFSET 0 -10 0
                  }
                }
              }
            }
            MOTION
            Frames: 1
            Frame Time: 0.033333
            0 100 0 0 0 0 90 0 0 0 0 0
            """;

        var clip = BvhHumanoidImporter.Import(bvh, renameJoint: BvhHumanoidImporter.RenameCmuJoint);
        await Assert.That(clip.Keys.Count).IsEqualTo(1);
        await Assert.That(clip.Keys[0].LocalRotations.ContainsKey(HumanoidBone.LeftUpLeg)).IsTrue();
        var q = clip.Keys[0].LocalRotations[HumanoidBone.LeftUpLeg];
        var expected = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, MathF.PI / 2f);
        var dot = MathF.Abs(Quaternion.Dot(Quaternion.Normalize(q), Quaternion.Normalize(expected)));
        await Assert.That(dot).IsGreaterThan(0.99f);
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
