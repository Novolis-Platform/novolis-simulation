using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

/// <summary>One bone influence on a vertex (weight should sum to ~1 across influences).</summary>
public readonly record struct VertexBoneWeight(HumanoidBone Bone, float Weight);

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

/// <summary>Linear-blend skinning on the CPU (apps / software rasterizers).</summary>
public static class CpuSkinDeformer
{
    /// <summary>
    /// Deforms bind vertices into <paramref name="destination"/> (length ≥ vertex count)
    /// using world bone matrices from <paramref name="world"/>.
    /// </summary>
    public static void Deform(SkinnedHumanoidMesh skin, HumanoidWorldPose world, Span<Vector3> destination)
    {
        var verts = skin.BindMesh.Vertices;
        if (destination.Length < verts.Length)
            throw new ArgumentException("Destination too short.", nameof(destination));

        Span<Matrix4x4> boneMats = stackalloc Matrix4x4[(int)HumanoidBone.Count];
        for (var i = 0; i < (int)HumanoidBone.Count; i++)
        {
            var bone = (HumanoidBone)i;
            var worldMat = Matrix4x4.CreateFromQuaternion(world.Rotation(bone)) *
                           Matrix4x4.CreateTranslation(world.Position(bone));
            boneMats[i] = skin.InverseBindPose[i] * worldMat;
        }

        for (var v = 0; v < verts.Length; v++)
        {
            var p = verts[v];
            var acc = Vector3.Zero;
            var wSum = 0f;
            foreach (var influence in skin.VertexWeights[v])
            {
                var skinned = Vector3.Transform(p, boneMats[(int)influence.Bone]);
                acc += skinned * influence.Weight;
                wSum += influence.Weight;
            }

            destination[v] = wSum > 1e-6f ? acc / wSum : p;
        }
    }

    /// <summary>Returns a new <see cref="TriangleMesh"/> with deformed positions (indices reused).</summary>
    public static TriangleMesh DeformToMesh(SkinnedHumanoidMesh skin, HumanoidWorldPose world)
    {
        var positions = new Vector3[skin.BindMesh.VertexCount];
        Deform(skin, world, positions);
        var indices = skin.BindMesh.Indices.ToArray();
        return new TriangleMesh(positions, indices);
    }
}
