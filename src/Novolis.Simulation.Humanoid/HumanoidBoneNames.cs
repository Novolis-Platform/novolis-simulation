namespace Novolis.Simulation.Humanoid;

/// <summary>Common Mixamo / Unity / Blender aliases for <see cref="HumanoidBone"/>.</summary>
public static class HumanoidBoneNames
{
    private static readonly Dictionary<string, HumanoidBone> Aliases =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Hips"] = HumanoidBone.Hips,
            ["mixamorig:Hips"] = HumanoidBone.Hips,
            ["Pelvis"] = HumanoidBone.Hips,
            ["Spine"] = HumanoidBone.Spine,
            ["mixamorig:Spine"] = HumanoidBone.Spine,
            ["Spine1"] = HumanoidBone.Spine1,
            ["mixamorig:Spine1"] = HumanoidBone.Spine1,
            ["Spine2"] = HumanoidBone.Spine2,
            ["mixamorig:Spine2"] = HumanoidBone.Spine2,
            ["Chest"] = HumanoidBone.Spine2,
            ["UpperChest"] = HumanoidBone.Spine2,
            ["Neck"] = HumanoidBone.Neck,
            ["mixamorig:Neck"] = HumanoidBone.Neck,
            ["Head"] = HumanoidBone.Head,
            ["mixamorig:Head"] = HumanoidBone.Head,
            ["LeftUpLeg"] = HumanoidBone.LeftUpLeg,
            ["mixamorig:LeftUpLeg"] = HumanoidBone.LeftUpLeg,
            ["LeftUpperLeg"] = HumanoidBone.LeftUpLeg,
            ["LeftLeg"] = HumanoidBone.LeftLeg,
            ["mixamorig:LeftLeg"] = HumanoidBone.LeftLeg,
            ["LeftLowerLeg"] = HumanoidBone.LeftLeg,
            ["LeftFoot"] = HumanoidBone.LeftFoot,
            ["mixamorig:LeftFoot"] = HumanoidBone.LeftFoot,
            ["LeftToeBase"] = HumanoidBone.LeftToeBase,
            ["mixamorig:LeftToeBase"] = HumanoidBone.LeftToeBase,
            ["LeftToes"] = HumanoidBone.LeftToeBase,
            ["RightUpLeg"] = HumanoidBone.RightUpLeg,
            ["mixamorig:RightUpLeg"] = HumanoidBone.RightUpLeg,
            ["RightUpperLeg"] = HumanoidBone.RightUpLeg,
            ["RightLeg"] = HumanoidBone.RightLeg,
            ["mixamorig:RightLeg"] = HumanoidBone.RightLeg,
            ["RightLowerLeg"] = HumanoidBone.RightLeg,
            ["RightFoot"] = HumanoidBone.RightFoot,
            ["mixamorig:RightFoot"] = HumanoidBone.RightFoot,
            ["RightToeBase"] = HumanoidBone.RightToeBase,
            ["mixamorig:RightToeBase"] = HumanoidBone.RightToeBase,
            ["RightToes"] = HumanoidBone.RightToeBase,
            ["LeftShoulder"] = HumanoidBone.LeftShoulder,
            ["mixamorig:LeftShoulder"] = HumanoidBone.LeftShoulder,
            ["LeftArm"] = HumanoidBone.LeftArm,
            ["mixamorig:LeftArm"] = HumanoidBone.LeftArm,
            ["LeftUpperArm"] = HumanoidBone.LeftArm,
            ["LeftForeArm"] = HumanoidBone.LeftForeArm,
            ["mixamorig:LeftForeArm"] = HumanoidBone.LeftForeArm,
            ["LeftLowerArm"] = HumanoidBone.LeftForeArm,
            ["LeftHand"] = HumanoidBone.LeftHand,
            ["mixamorig:LeftHand"] = HumanoidBone.LeftHand,
            ["RightShoulder"] = HumanoidBone.RightShoulder,
            ["mixamorig:RightShoulder"] = HumanoidBone.RightShoulder,
            ["RightArm"] = HumanoidBone.RightArm,
            ["mixamorig:RightArm"] = HumanoidBone.RightArm,
            ["RightUpperArm"] = HumanoidBone.RightArm,
            ["RightForeArm"] = HumanoidBone.RightForeArm,
            ["mixamorig:RightForeArm"] = HumanoidBone.RightForeArm,
            ["RightLowerArm"] = HumanoidBone.RightForeArm,
            ["RightHand"] = HumanoidBone.RightHand,
            ["mixamorig:RightHand"] = HumanoidBone.RightHand,
        };

    /// <summary>Canonical Mixamo-style name for a bone.</summary>
    public static string Canonical(HumanoidBone bone) => bone switch
    {
        HumanoidBone.Hips => "Hips",
        HumanoidBone.Spine => "Spine",
        HumanoidBone.Spine1 => "Spine1",
        HumanoidBone.Spine2 => "Spine2",
        HumanoidBone.Neck => "Neck",
        HumanoidBone.Head => "Head",
        HumanoidBone.LeftUpLeg => "LeftUpLeg",
        HumanoidBone.LeftLeg => "LeftLeg",
        HumanoidBone.LeftFoot => "LeftFoot",
        HumanoidBone.LeftToeBase => "LeftToeBase",
        HumanoidBone.RightUpLeg => "RightUpLeg",
        HumanoidBone.RightLeg => "RightLeg",
        HumanoidBone.RightFoot => "RightFoot",
        HumanoidBone.RightToeBase => "RightToeBase",
        HumanoidBone.LeftShoulder => "LeftShoulder",
        HumanoidBone.LeftArm => "LeftArm",
        HumanoidBone.LeftForeArm => "LeftForeArm",
        HumanoidBone.LeftHand => "LeftHand",
        HumanoidBone.RightShoulder => "RightShoulder",
        HumanoidBone.RightArm => "RightArm",
        HumanoidBone.RightForeArm => "RightForeArm",
        HumanoidBone.RightHand => "RightHand",
        _ => bone.ToString(),
    };

    /// <summary>Resolves an imported bone name to the standard enum.</summary>
    public static bool TryResolve(string name, out HumanoidBone bone)
    {
        if (Aliases.TryGetValue(name, out bone))
            return true;

        // Strip common prefixes: mixamorig_, Armature|
        var trimmed = name;
        var colon = trimmed.LastIndexOf(':');
        if (colon >= 0 && colon + 1 < trimmed.Length)
            trimmed = trimmed[(colon + 1)..];
        var pipe = trimmed.LastIndexOf('|');
        if (pipe >= 0 && pipe + 1 < trimmed.Length)
            trimmed = trimmed[(pipe + 1)..];

        return Aliases.TryGetValue(trimmed, out bone);
    }
}
