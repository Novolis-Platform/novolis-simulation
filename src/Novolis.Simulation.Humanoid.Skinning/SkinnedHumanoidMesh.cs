using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

/// <summary>Bind mesh plus per-vertex weights and inverse-bind matrices for CPU skinning.</summary>
public sealed class SkinnedHumanoidMesh
{
    /// <summary>Creates a skinned mesh description.</summary>
    public SkinnedHumanoidMesh(
        TriangleMesh bindMesh,
        IReadOnlyList<VertexBoneWeight[]> vertexWeights,
        Matrix4x4[] inverseBindPose)
    {
        ArgumentNullException.ThrowIfNull(bindMesh);
        ArgumentNullException.ThrowIfNull(vertexWeights);
        ArgumentNullException.ThrowIfNull(inverseBindPose);
        if (vertexWeights.Count != bindMesh.VertexCount)
            throw new ArgumentException("Weight list length must match vertex count.", nameof(vertexWeights));
        if (inverseBindPose.Length != (int)HumanoidBone.Count)
            throw new ArgumentException($"Expected {(int)HumanoidBone.Count} inverse-bind matrices.", nameof(inverseBindPose));

        BindMesh = bindMesh;
        VertexWeights = vertexWeights;
        InverseBindPose = inverseBindPose;
    }

    /// <summary>Rest-pose triangle mesh.</summary>
    public TriangleMesh BindMesh { get; }

    /// <summary>Per-vertex bone weights (typically ≤4 influences).</summary>
    public IReadOnlyList<VertexBoneWeight[]> VertexWeights { get; }

    /// <summary>Inverse bind matrices indexed by <see cref="HumanoidBone"/>.</summary>
    public Matrix4x4[] InverseBindPose { get; }

    /// <summary>
    /// Builds identity inverse binds from a <see cref="HumanoidBindPose"/> (translation-only approx).
    /// </summary>
    public static Matrix4x4[] CreateTranslationInverseBinds(HumanoidBindPose bind)
    {
        var mats = new Matrix4x4[(int)HumanoidBone.Count];
        for (var i = 0; i < mats.Length; i++)
        {
            var p = bind[(HumanoidBone)i];
            mats[i] = Matrix4x4.CreateTranslation(-p);
        }

        return mats;
    }
}
