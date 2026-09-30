using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

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

    /// <summary>
    /// Translation-only blend: each influence moves the vertex by the bone's joint delta
    /// (<c>worldPos - bindPos</c>). Safe for auto-skin + Soft-IK when hierarchical rotations
    /// are not reliable (avoids the classic LBS explode from mismatched FromTo rotations).
    /// </summary>
    public static void DeformTranslations(
        SkinnedHumanoidMesh skin,
        HumanoidBindPose bind,
        HumanoidWorldPose world,
        Span<Vector3> destination)
    {
        ArgumentNullException.ThrowIfNull(skin);
        ArgumentNullException.ThrowIfNull(bind);
        ArgumentNullException.ThrowIfNull(world);
        var verts = skin.BindMesh.Vertices;
        if (destination.Length < verts.Length)
            throw new ArgumentException("Destination too short.", nameof(destination));

        Span<Vector3> delta = stackalloc Vector3[(int)HumanoidBone.Count];
        for (var i = 0; i < (int)HumanoidBone.Count; i++)
        {
            var bone = (HumanoidBone)i;
            delta[i] = world.Position(bone) - bind[bone];
        }

        for (var v = 0; v < verts.Length; v++)
        {
            var p = verts[v];
            var acc = Vector3.Zero;
            var wSum = 0f;
            foreach (var influence in skin.VertexWeights[v])
            {
                acc += (p + delta[(int)influence.Bone]) * influence.Weight;
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
