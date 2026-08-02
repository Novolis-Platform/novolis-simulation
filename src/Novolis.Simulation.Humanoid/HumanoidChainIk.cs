using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>
/// Runs <see cref="FabrikChain"/> over a humanoid bone path and writes positions + aim rotations
/// into <see cref="HumanoidWorldPose"/> (same rotation convention as bind-aware <see cref="TwoBoneIk"/>).
/// </summary>
public static class HumanoidChainIk
{
    /// <summary>
    /// FABRIK along <paramref name="bones"/> (root → tip). Root bone stays pinned when
    /// <paramref name="pinRoot"/> is true. Segment lengths default to current world distances.
    /// </summary>
    public static bool Apply(
        HumanoidWorldPose world,
        HumanoidBindPose bind,
        ReadOnlySpan<HumanoidBone> bones,
        Vector3 tipTarget,
        bool pinRoot = true,
        int maxIterations = 16,
        float tolerance = 1e-4f,
        Span<float> lengthScratch = default)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(bind);
        if (bones.Length < 2)
            throw new ArgumentException("Need at least two bones (root and tip).", nameof(bones));

        Span<Vector3> positions = stackalloc Vector3[bones.Length];
        for (var i = 0; i < bones.Length; i++)
            positions[i] = world.Position(bones[i]);

        var segmentCount = bones.Length - 1;
        Span<float> lengths = lengthScratch.Length >= segmentCount
            ? lengthScratch[..segmentCount]
            : stackalloc float[segmentCount];

        for (var i = 0; i < lengths.Length; i++)
        {
            var from = bones[i];
            var to = bones[i + 1];
            // Prefer bind rest lengths so FABRIK stays stable after prior IK edits.
            lengths[i] = MathF.Max(1e-4f, Vector3.Distance(bind[from], bind[to]));
        }

        var reached = FabrikChain.Solve(positions, lengths, tipTarget, pinRoot, maxIterations, tolerance);

        for (var i = 0; i < bones.Length - 1; i++)
        {
            var bone = bones[i];
            var child = bones[i + 1];
            var bindDir = bind[child] - bind[bone];
            var curDir = positions[i + 1] - positions[i];
            var rot = TwoBoneIk.FromToRotation(bindDir, curDir);
            world.Set(bone, positions[i], rot);
        }

        var tip = bones[^1];
        var tipParent = bones[^2];
        var tipRot = world.Rotation(tipParent);
        world.Set(tip, positions[^1], tipRot);
        return reached;
    }
}
