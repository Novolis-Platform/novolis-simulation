using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>Rest (bind) pose: world translations of each bone in T-pose, meters.</summary>
public sealed class HumanoidBindPose
{
    private readonly Vector3[] _world = new Vector3[(int)HumanoidBone.Count];

    /// <summary>Character height used to scale the default proportion set.</summary>
    public float HeightMeters { get; }

    /// <summary>World-space bind positions.</summary>
    public ReadOnlySpan<Vector3> WorldPositions => _world;

    private HumanoidBindPose(float heightMeters) => HeightMeters = heightMeters;

    /// <summary>Gets bind world position for a bone.</summary>
    public Vector3 this[HumanoidBone bone] => _world[(int)bone];

    /// <summary>
    /// Default adult T-pose: facing +Z, up +Y, arms out along ±X, centered on hips at Y≈0.92 for 1.8 m.
    /// </summary>
    public static HumanoidBindPose CreateDefaultTPose(float heightMeters = 1.8f)
    {
        if (heightMeters < 0.5f || heightMeters > 3f)
            throw new ArgumentOutOfRangeException(nameof(heightMeters), heightMeters, "Expected 0.5–3 m.");

        var s = heightMeters / 1.8f;
        var pose = new HumanoidBindPose(heightMeters);

        void Set(HumanoidBone bone, float x, float y, float z) =>
            pose._world[(int)bone] = new Vector3(x * s, y * s, z * s);

        // Proportions roughly Mixamo / mannequin at 1.8 m.
        Set(HumanoidBone.Hips, 0f, 0.92f, 0f);
        Set(HumanoidBone.Spine, 0f, 1.05f, 0f);
        Set(HumanoidBone.Spine1, 0f, 1.18f, 0f);
        Set(HumanoidBone.Spine2, 0f, 1.32f, 0f);
        Set(HumanoidBone.Neck, 0f, 1.48f, 0f);
        Set(HumanoidBone.Head, 0f, 1.62f, 0f);

        // Hip sockets ~16 cm apart so thighs don't read as a single crotch point.
        Set(HumanoidBone.LeftUpLeg, -0.16f, 0.9f, 0f);
        Set(HumanoidBone.LeftLeg, -0.17f, 0.48f, 0f);
        Set(HumanoidBone.LeftFoot, -0.17f, 0.08f, 0.04f);
        Set(HumanoidBone.LeftToeBase, -0.17f, 0.02f, 0.14f);

        Set(HumanoidBone.RightUpLeg, 0.16f, 0.9f, 0f);
        Set(HumanoidBone.RightLeg, 0.17f, 0.48f, 0f);
        Set(HumanoidBone.RightFoot, 0.17f, 0.08f, 0.04f);
        Set(HumanoidBone.RightToeBase, 0.17f, 0.02f, 0.14f);

        Set(HumanoidBone.LeftShoulder, -0.06f, 1.38f, 0f);
        Set(HumanoidBone.LeftArm, -0.22f, 1.36f, 0f);
        Set(HumanoidBone.LeftForeArm, -0.48f, 1.36f, 0f);
        Set(HumanoidBone.LeftHand, -0.72f, 1.36f, 0f);

        Set(HumanoidBone.RightShoulder, 0.06f, 1.38f, 0f);
        Set(HumanoidBone.RightArm, 0.22f, 1.36f, 0f);
        Set(HumanoidBone.RightForeArm, 0.48f, 1.36f, 0f);
        Set(HumanoidBone.RightHand, 0.72f, 1.36f, 0f);

        return pose;
    }

    /// <summary>
    /// Builds a bind pose from explicit world positions (meters). Bones with
    /// <paramref name="present"/> false are filled from <see cref="CreateDefaultTPose"/>.
    /// </summary>
    public static HumanoidBindPose FromWorldPositions(
        ReadOnlySpan<Vector3> worldPositions,
        ReadOnlySpan<bool> present,
        float heightMeters = 1.8f)
    {
        if (worldPositions.Length < (int)HumanoidBone.Count || present.Length < (int)HumanoidBone.Count)
            throw new ArgumentException($"Expected {(int)HumanoidBone.Count} positions/flags.");

        var fallback = CreateDefaultTPose(heightMeters);
        var pose = new HumanoidBindPose(heightMeters);
        for (var i = 0; i < (int)HumanoidBone.Count; i++)
            pose._world[i] = present[i] ? worldPositions[i] : fallback[(HumanoidBone)i];
        return pose;
    }

    /// <summary>Bind-space bone length from parent to bone (hips length is 0).</summary>
    public float BoneLength(HumanoidBone bone)
    {
        var parent = HumanoidHierarchy.ParentBone(bone);
        if (parent is null)
            return 0f;
        return Vector3.Distance(this[parent.Value], this[bone]);
    }
}
