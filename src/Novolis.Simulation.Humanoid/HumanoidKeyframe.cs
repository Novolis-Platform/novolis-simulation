using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>One keyframe of local rotations (+ optional root translation).</summary>
public sealed class HumanoidKeyframe
{
    /// <summary>Time in seconds from clip start.</summary>
    public float TimeSeconds { get; set; }

    /// <summary>Optional root translation; when null, sampler keeps previous / bind hips.</summary>
    public Vector3? RootTranslation { get; set; }

    /// <summary>Local rotations keyed by bone (missing bones = identity).</summary>
    public Dictionary<HumanoidBone, Quaternion> LocalRotations { get; set; } = new();
}
