namespace Novolis.Simulation.Humanoid;

/// <summary>
/// Maps standard <see cref="HumanoidBone"/> joints onto <c>Novolis.Physics.Joints.RagdollHumanoidPreset</c>
/// sphere indices (documented constants; no package reference required).
/// </summary>
/// <remarks>
/// Ragdoll uses 11 spheres; several Mixamo bones collapse onto the same sphere.
/// Full bridge that copies transforms lives in a follow-on adapter.
/// </remarks>
public static class HumanoidRagdollMap
{
    /// <summary>Ragdoll sphere index for hip.</summary>
    public const int RagdollHip = 0;

    /// <summary>Ragdoll sphere index for left knee.</summary>
    public const int RagdollLeftKnee = 1;

    /// <summary>Ragdoll sphere index for right knee.</summary>
    public const int RagdollRightKnee = 2;

    /// <summary>Ragdoll sphere index for chest.</summary>
    public const int RagdollChest = 3;

    /// <summary>Ragdoll sphere index for head.</summary>
    public const int RagdollHead = 4;

    /// <summary>Ragdoll sphere index for left shoulder.</summary>
    public const int RagdollLeftShoulder = 5;

    /// <summary>Ragdoll sphere index for right shoulder.</summary>
    public const int RagdollRightShoulder = 6;

    /// <summary>Ragdoll sphere index for left hand.</summary>
    public const int RagdollLeftHand = 7;

    /// <summary>Ragdoll sphere index for right hand.</summary>
    public const int RagdollRightHand = 8;

    /// <summary>Ragdoll sphere index for left foot.</summary>
    public const int RagdollLeftFoot = 9;

    /// <summary>Ragdoll sphere index for right foot.</summary>
    public const int RagdollRightFoot = 10;

    /// <summary>Best-effort sphere for a humanoid bone, or -1 if none.</summary>
    public static int ToRagdollSphere(HumanoidBone bone) => bone switch
    {
        HumanoidBone.Hips or HumanoidBone.Spine => RagdollHip,
        HumanoidBone.Spine1 or HumanoidBone.Spine2 => RagdollChest,
        HumanoidBone.Neck or HumanoidBone.Head => RagdollHead,
        HumanoidBone.LeftUpLeg or HumanoidBone.LeftLeg => RagdollLeftKnee,
        HumanoidBone.LeftFoot or HumanoidBone.LeftToeBase => RagdollLeftFoot,
        HumanoidBone.RightUpLeg or HumanoidBone.RightLeg => RagdollRightKnee,
        HumanoidBone.RightFoot or HumanoidBone.RightToeBase => RagdollRightFoot,
        HumanoidBone.LeftShoulder or HumanoidBone.LeftArm => RagdollLeftShoulder,
        HumanoidBone.LeftForeArm or HumanoidBone.LeftHand => RagdollLeftHand,
        HumanoidBone.RightShoulder or HumanoidBone.RightArm => RagdollRightShoulder,
        HumanoidBone.RightForeArm or HumanoidBone.RightHand => RagdollRightHand,
        _ => -1,
    };
}
