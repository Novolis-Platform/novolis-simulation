using System.Numerics;
using System.Runtime.InteropServices;
using Novolis.Math.Geometry;
using Novolis.Physics.Collision.Simple;
using Novolis.Physics.Joints;
using Novolis.Simulation.Humanoid.Skinning;

namespace Novolis.Simulation.Humanoid.Tests;

public class RagdollAdaptiveBodyTests
{
    [Test]
    public async Task AdaptiveBody_FollowsRagdollSpheres()
    {
        var spheres = new List<SphereState>();
        var joints = new List<DistanceJoint>();
        var swings = new List<SwingLimit>();
        var hinges = new List<HingeLimit>();
        RagdollHumanoidPreset.BuildStanding(new Vector3(0f, 0f, 0f), spheres, joints, swings, hinges);

        var centers = new Vector3[spheres.Count];
        for (var i = 0; i < spheres.Count; i++)
            centers[i] = spheres[i].Position;

        var body = HumanoidAdaptiveBody.CreateFromRagdollBind(centers);
        await Assert.That(body.VertexCount).IsGreaterThan(20);
        await Assert.That(body.TriangleCount).IsGreaterThan(20);

        var handles = new Vector3[HumanoidAdaptiveBody.SphereCount];
        HumanoidAdaptiveBody.CopySphereCenters(centers, handles);
        var mesh0 = body.AdaptToMesh(handles);

        for (var i = 0; i < spheres.Count; i++)
            spheres[i].Position += new Vector3(1f, 0f, 0f);
        for (var i = 0; i < spheres.Count; i++)
            centers[i] = spheres[i].Position;
        HumanoidAdaptiveBody.CopySphereCenters(centers, handles);
        var mesh1 = body.AdaptToMesh(handles);

        var delta = mesh1.Vertices[0] - mesh0.Vertices[0];
        await Assert.That(delta.X).IsEqualTo(1f).Within(0.05f);
    }

    [Test]
    public async Task Ragdoll_TipsAndSettles_WithoutEnergyGain()
    {
        const float radius = 0.2f;
        var sim = new ConstrainedSphereSimulator
        {
            Options =
            {
                Radius = radius,
                LinearDragPerSecond = 1.35,
                SphereRestitution = 0.0f,
                StaticRestitution = 0.0f,
                GroundFrictionPerSecond = 22.0,
                SleepSpeedThreshold = 0.12f,
                MaxSpeedMps = 6f,
                FloorHeight = 0f,
                GroundContactSlack = 0.08f,
            },
            JointIterations = 20,
            JointRelaxIterations = 6,
            AngularIterations = 0,
            InternalCollisionIterations = 0,
            ConstraintPasses = 2,
        };

        var spheres = new List<SphereState>();
        var joints = new List<DistanceJoint>();
        var swings = new List<SwingLimit>();
        var hinges = new List<HingeLimit>();
        RagdollHumanoidPreset.BuildStanding(new Vector3(0f, 0f, 0f), spheres, joints, swings, hinges);

        var floor = BuildFloor(20f);
        var clamp = new InteriorClampVolume
        {
            MinX = -8f,
            MaxX = 8f,
            MinY = radius,
            MaxY = 6f,
            MinZ = -8f,
            MaxZ = 8f,
        };

        sim.SetJoints(CollectionsMarshal.AsSpan(joints));
        sim.DepenetrateSpawnedRange(spheres, 0, spheres.Count, clamp);
        RagdollHumanoidPreset.StabilizeSpawn(
            spheres,
            CollectionsMarshal.AsSpan(joints),
            clamp,
            sim,
            spawnStiffness: 0.85f);

        var prev = new Vector3[spheres.Count];
        spheres[RagdollHumanoidPreset.Chest].Velocity += new Vector3(1.6f, 0.35f, 0.1f);
        foreach (var s in spheres)
            s.IsSleeping = false;
        sim.MarkPileUnsettled();

        const float dt = 1f / 60f;
        const float entropy = 3.2f;
        float maxSp = 0f;
        for (var i = 0; i < 240; i++)
        {
            for (var s = 0; s < spheres.Count; s++)
                prev[s] = spheres[s].Position;

            sim.SetJoints(CollectionsMarshal.AsSpan(joints));
            sim.Step(floor, spheres, clamp, dt);

            var invDt = 1f / dt;
            var damp = MathF.Exp(-entropy * dt);
            for (var s = 0; s < spheres.Count; s++)
            {
                var fromPos = (spheres[s].Position - prev[s]) * invDt;
                spheres[s].Velocity = Vector3.Lerp(spheres[s].Velocity, fromPos, 0.65f);
                spheres[s].Velocity *= damp;
                if (spheres[s].Position.Y <= radius + 0.12f)
                    spheres[s].Velocity *= MathF.Exp(-4.5f * dt);
                maxSp = System.Math.Max(maxSp, spheres[s].Velocity.Length());
            }

            var quiet = true;
            foreach (var s in spheres)
            {
                if (s.Velocity.Length() >= 0.15f)
                    quiet = false;
            }

            if (quiet)
            {
                foreach (var s in spheres)
                {
                    s.Velocity = Vector3.Zero;
                    s.IsSleeping = true;
                    s.IsGrounded = true;
                }
            }
        }

        float endSp = 0f;
        var sleeping = 0;
        foreach (var s in spheres)
        {
            endSp = System.Math.Max(endSp, s.Velocity.Length());
            if (s.IsSleeping)
                sleeping++;
        }

        await Assert.That(endSp).IsLessThan(0.2f);
        await Assert.That(sleeping).IsEqualTo(spheres.Count);
        await Assert.That(maxSp).IsLessThan(8f);
    }

    private static BvhStaticWorld BuildFloor(float half)
    {
        var verts = new[]
        {
            new Vector3(-half, 0f, -half),
            new Vector3(half, 0f, -half),
            new Vector3(half, 0f, half),
            new Vector3(-half, 0f, half),
        };
        var indices = new[] { 0, 1, 2, 0, 2, 3 };
        return new BvhStaticWorld(new TriangleMesh(verts, indices));
    }
}
