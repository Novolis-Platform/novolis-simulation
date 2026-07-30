namespace Novolis.Simulation.Humanoid;

/// <summary>Parent indices for <see cref="HumanoidBone"/> (-1 = root).</summary>
public static class HumanoidHierarchy
{
    private static readonly sbyte[] Parents =
    [
        -1, // Hips
        0,  // Spine
        1,  // Spine1
        2,  // Spine2
        3,  // Neck
        4,  // Head
        0,  // LeftUpLeg
        6,  // LeftLeg
        7,  // LeftFoot
        8,  // LeftToeBase
        0,  // RightUpLeg
        10, // RightLeg
        11, // RightFoot
        12, // RightToeBase
        3,  // LeftShoulder
        14, // LeftArm
        15, // LeftForeArm
        16, // LeftHand
        3,  // RightShoulder
        18, // RightArm
        19, // RightForeArm
        20, // RightHand
    ];

    /// <summary>Parent bone index, or -1 for <see cref="HumanoidBone.Hips"/>.</summary>
    public static int Parent(HumanoidBone bone) => Parents[(int)bone];

    /// <summary>Parent bone, or <c>null</c> for hips.</summary>
    public static HumanoidBone? ParentBone(HumanoidBone bone)
    {
        var p = Parent(bone);
        return p < 0 ? null : (HumanoidBone)p;
    }

    /// <summary>True when <paramref name="bone"/> is a required Unity-style core bone (excludes toes).</summary>
    public static bool IsCoreRequired(HumanoidBone bone) => bone switch
    {
        HumanoidBone.LeftToeBase or HumanoidBone.RightToeBase => false,
        HumanoidBone.Count => false,
        _ => true,
    };
}
