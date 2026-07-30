using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>Debug / stick-figure segment between two bones.</summary>
public readonly record struct HumanoidBoneSegment(HumanoidBone From, HumanoidBone To, Vector3 Start, Vector3 End);

/// <summary>Builds line segments for drawing a solved humanoid.</summary>
public static class HumanoidDebugDraw
{
    private static readonly (HumanoidBone From, HumanoidBone To)[] Edges =
    [
        (HumanoidBone.Hips, HumanoidBone.Spine),
        (HumanoidBone.Spine, HumanoidBone.Spine1),
        (HumanoidBone.Spine1, HumanoidBone.Spine2),
        (HumanoidBone.Spine2, HumanoidBone.Neck),
        (HumanoidBone.Neck, HumanoidBone.Head),
        (HumanoidBone.Hips, HumanoidBone.LeftUpLeg),
        (HumanoidBone.LeftUpLeg, HumanoidBone.LeftLeg),
        (HumanoidBone.LeftLeg, HumanoidBone.LeftFoot),
        (HumanoidBone.LeftFoot, HumanoidBone.LeftToeBase),
        (HumanoidBone.Hips, HumanoidBone.RightUpLeg),
        (HumanoidBone.RightUpLeg, HumanoidBone.RightLeg),
        (HumanoidBone.RightLeg, HumanoidBone.RightFoot),
        (HumanoidBone.RightFoot, HumanoidBone.RightToeBase),
        (HumanoidBone.Spine2, HumanoidBone.LeftShoulder),
        (HumanoidBone.LeftShoulder, HumanoidBone.LeftArm),
        (HumanoidBone.LeftArm, HumanoidBone.LeftForeArm),
        (HumanoidBone.LeftForeArm, HumanoidBone.LeftHand),
        (HumanoidBone.Spine2, HumanoidBone.RightShoulder),
        (HumanoidBone.RightShoulder, HumanoidBone.RightArm),
        (HumanoidBone.RightArm, HumanoidBone.RightForeArm),
        (HumanoidBone.RightForeArm, HumanoidBone.RightHand),
    ];

    /// <summary>Returns bone-to-bone segments from a solved world pose.</summary>
    public static HumanoidBoneSegment[] BuildSegments(HumanoidWorldPose world)
    {
        var result = new HumanoidBoneSegment[Edges.Length];
        for (var i = 0; i < Edges.Length; i++)
        {
            var (from, to) = Edges[i];
            result[i] = new HumanoidBoneSegment(from, to, world.Position(from), world.Position(to));
        }

        return result;
    }
}
