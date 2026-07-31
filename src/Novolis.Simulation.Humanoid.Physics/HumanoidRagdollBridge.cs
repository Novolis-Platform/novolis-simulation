using System.Numerics;
using Novolis.Physics.Collision.Simple;
using Novolis.Physics.Joints;

namespace Novolis.Simulation.Humanoid.Physics;

/// <summary>
/// Copies transforms between a Mixamo-style <see cref="HumanoidWorldPose"/> and
/// <see cref="RagdollHumanoidPreset"/> sphere lists (11 spheres).
/// </summary>
public static class HumanoidRagdollBridge
{
    /// <summary>Required sphere count for the humanoid ragdoll preset.</summary>
    public const int SphereCount = RagdollHumanoidPreset.SphereCount;

    /// <summary>
    /// Builds a standing ragdoll from <see cref="RagdollHumanoidPreset"/> then overwrites
    /// sphere centers from the bind pose (scaled proportions).
    /// </summary>
    public static void BuildStandingFromBind(
        HumanoidBindPose bind,
        IList<SphereState> spheres,
        IList<DistanceJoint> joints,
        IList<SwingLimit> swingLimits,
        IList<HingeLimit> hingeLimits,
        float runtimeStiffness = 0.65f)
    {
        var ground = bind[HumanoidBone.LeftFoot] with { Y = 0f };
        ground = new Vector3(bind[HumanoidBone.Hips].X, 0f, bind[HumanoidBone.Hips].Z);
        RagdollHumanoidPreset.BuildStanding(ground, spheres, joints, swingLimits, hingeLimits, runtimeStiffness);
        ApplyBindToSpheres(bind, spheres);
        RelengthJoints(spheres, joints);
        // Limits were authored for the standing layout; rebuild so rest frames match bind proportions
        // (otherwise angular solvers fight distance joints and inject jitter / stretch).
        RagdollHumanoidPreset.BuildLimits(spheres, swingLimits, hingeLimits, runtimeStiffness);
    }

    /// <summary>Writes bind-pose bone positions into ragdoll spheres.</summary>
    public static void ApplyBindToSpheres(HumanoidBindPose bind, IList<SphereState> spheres)
    {
        EnsureSphereCount(spheres);
        WriteBone(spheres, HumanoidRagdollMap.RagdollHip, bind[HumanoidBone.Hips]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollChest, bind[HumanoidBone.Spine2]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollHead, bind[HumanoidBone.Head]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftKnee, bind[HumanoidBone.LeftLeg]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightKnee, bind[HumanoidBone.RightLeg]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftFoot, bind[HumanoidBone.LeftFoot]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightFoot, bind[HumanoidBone.RightFoot]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftShoulder, bind[HumanoidBone.LeftArm]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightShoulder, bind[HumanoidBone.RightArm]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftHand, bind[HumanoidBone.LeftHand]);
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightHand, bind[HumanoidBone.RightHand]);
    }

    /// <summary>Writes a solved humanoid world pose into ragdoll sphere centers.</summary>
    public static void ApplyWorldPoseToSpheres(HumanoidWorldPose world, IList<SphereState> spheres)
    {
        EnsureSphereCount(spheres);
        WriteBone(spheres, HumanoidRagdollMap.RagdollHip, world.Position(HumanoidBone.Hips));
        WriteBone(spheres, HumanoidRagdollMap.RagdollChest, world.Position(HumanoidBone.Spine2));
        WriteBone(spheres, HumanoidRagdollMap.RagdollHead, world.Position(HumanoidBone.Head));
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftKnee, world.Position(HumanoidBone.LeftLeg));
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightKnee, world.Position(HumanoidBone.RightLeg));
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftFoot, world.Position(HumanoidBone.LeftFoot));
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightFoot, world.Position(HumanoidBone.RightFoot));
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftShoulder, world.Position(HumanoidBone.LeftArm));
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightShoulder, world.Position(HumanoidBone.RightArm));
        WriteBone(spheres, HumanoidRagdollMap.RagdollLeftHand, world.Position(HumanoidBone.LeftHand));
        WriteBone(spheres, HumanoidRagdollMap.RagdollRightHand, world.Position(HumanoidBone.RightHand));
    }

    /// <summary>
    /// Samples sphere centers into a <see cref="HumanoidWorldPose"/> (coarse inverse map).
    /// Missing intermediate bones are lerped along the chain.
    /// </summary>
    public static HumanoidWorldPose WorldPoseFromSpheres(IList<SphereState> spheres)
    {
        EnsureSphereCount(spheres);
        var world = new HumanoidWorldPose();
        var identity = Quaternion.Identity;

        void Set(HumanoidBone bone, Vector3 p) => world.Set(bone, p, identity);

        var hips = spheres[HumanoidRagdollMap.RagdollHip].Position;
        var chest = spheres[HumanoidRagdollMap.RagdollChest].Position;
        var head = spheres[HumanoidRagdollMap.RagdollHead].Position;

        Set(HumanoidBone.Hips, hips);
        Set(HumanoidBone.Spine, Vector3.Lerp(hips, chest, 0.33f));
        Set(HumanoidBone.Spine1, Vector3.Lerp(hips, chest, 0.66f));
        Set(HumanoidBone.Spine2, chest);
        Set(HumanoidBone.Neck, Vector3.Lerp(chest, head, 0.4f));
        Set(HumanoidBone.Head, head);

        var lKnee = spheres[HumanoidRagdollMap.RagdollLeftKnee].Position;
        var rKnee = spheres[HumanoidRagdollMap.RagdollRightKnee].Position;
        var lFoot = spheres[HumanoidRagdollMap.RagdollLeftFoot].Position;
        var rFoot = spheres[HumanoidRagdollMap.RagdollRightFoot].Position;
        Set(HumanoidBone.LeftUpLeg, Vector3.Lerp(hips, lKnee, 0.15f));
        Set(HumanoidBone.LeftLeg, lKnee);
        Set(HumanoidBone.LeftFoot, lFoot);
        Set(HumanoidBone.LeftToeBase, lFoot + new Vector3(0f, 0f, 0.1f));
        Set(HumanoidBone.RightUpLeg, Vector3.Lerp(hips, rKnee, 0.15f));
        Set(HumanoidBone.RightLeg, rKnee);
        Set(HumanoidBone.RightFoot, rFoot);
        Set(HumanoidBone.RightToeBase, rFoot + new Vector3(0f, 0f, 0.1f));

        var lShoulder = spheres[HumanoidRagdollMap.RagdollLeftShoulder].Position;
        var rShoulder = spheres[HumanoidRagdollMap.RagdollRightShoulder].Position;
        var lHand = spheres[HumanoidRagdollMap.RagdollLeftHand].Position;
        var rHand = spheres[HumanoidRagdollMap.RagdollRightHand].Position;
        Set(HumanoidBone.LeftShoulder, Vector3.Lerp(chest, lShoulder, 0.35f));
        Set(HumanoidBone.LeftArm, lShoulder);
        Set(HumanoidBone.LeftForeArm, Vector3.Lerp(lShoulder, lHand, 0.55f));
        Set(HumanoidBone.LeftHand, lHand);
        Set(HumanoidBone.RightShoulder, Vector3.Lerp(chest, rShoulder, 0.35f));
        Set(HumanoidBone.RightArm, rShoulder);
        Set(HumanoidBone.RightForeArm, Vector3.Lerp(rShoulder, rHand, 0.55f));
        Set(HumanoidBone.RightHand, rHand);

        return world;
    }

    private static void RelengthJoints(IList<SphereState> spheres, IList<DistanceJoint> joints)
    {
        for (var i = 0; i < joints.Count; i++)
        {
            var j = joints[i];
            var rest = Vector3.Distance(spheres[j.SphereA].Position, spheres[j.SphereB].Position);
            joints[i] = new DistanceJoint(j.SphereA, j.SphereB, rest, j.Stiffness);
        }
    }

    private static void WriteBone(IList<SphereState> spheres, int index, Vector3 position)
    {
        spheres[index].Position = position;
        spheres[index].Velocity = Vector3.Zero;
    }

    private static void EnsureSphereCount(IList<SphereState> spheres)
    {
        if (spheres.Count < SphereCount)
            throw new ArgumentException($"Expected at least {SphereCount} spheres, got {spheres.Count}.", nameof(spheres));
    }
}
