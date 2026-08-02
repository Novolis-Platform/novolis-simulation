using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

/// <summary>
/// Auto-skins an unrigged mesh by assigning nearest-bone linear-blend weights
/// against a <see cref="HumanoidBindPose"/>.
/// </summary>
public static class HumanoidNearestBoneSkinner
{
    /// <summary>
    /// Builds a <see cref="SkinnedHumanoidMesh"/> with up to <paramref name="influences"/> bones per vertex
    /// (distance to bone shafts / joints, falloff, normalized). Uses translation-only inverse binds.
    /// </summary>
    public static SkinnedHumanoidMesh Bind(
        TriangleMesh mesh,
        HumanoidBindPose bind,
        int influences = 4)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(bind);
        if (influences < 1 || influences > 8)
            throw new ArgumentOutOfRangeException(nameof(influences), "Expected 1–8 influences.");
        if (mesh.VertexCount == 0)
            throw new ArgumentException("Mesh has no vertices.", nameof(mesh));

        var boneCount = (int)HumanoidBone.Count;
        var joints = new Vector3[boneCount];
        for (var i = 0; i < boneCount; i++)
            joints[i] = bind[(HumanoidBone)i];

        var weights = new VertexBoneWeight[mesh.VertexCount][];
        var verts = mesh.Vertices;
        Span<(int Bone, float DistSq)> nearest = stackalloc (int, float)[influences];

        for (var v = 0; v < mesh.VertexCount; v++)
        {
            var p = verts[v];
            for (var i = 0; i < influences; i++)
                nearest[i] = (-1, float.MaxValue);

            // Prefer limb shafts over hips/spine when a vertex sits near an arm/leg —
            // otherwise clothing near the torso steals arm verts and motion looks frozen.
            var nearLimb = MinLimbShaftDistSq(p, bind, joints) < 0.14f * 0.14f;
            for (var b = 0; b < boneCount; b++)
            {
                var bone = (HumanoidBone)b;
                var d = DistanceToBoneSq(p, bind, bone, joints);
                if (nearLimb && IsCoreTorso(bone))
                    d *= 9f;
                for (var i = 0; i < influences; i++)
                {
                    if (d >= nearest[i].DistSq)
                        continue;
                    for (var j = influences - 1; j > i; j--)
                        nearest[j] = nearest[j - 1];
                    nearest[i] = (b, d);
                    break;
                }
            }

            var list = new List<VertexBoneWeight>(influences);
            var wSum = 0f;
            for (var i = 0; i < influences; i++)
            {
                if (nearest[i].Bone < 0)
                    continue;
                var dist = MathF.Sqrt(nearest[i].DistSq);
                var w = 1f / MathF.Max(dist, 1e-4f);
                wSum += w;
                list.Add(new VertexBoneWeight((HumanoidBone)nearest[i].Bone, w));
            }

            if (list.Count == 0 || wSum < 1e-8f)
            {
                weights[v] = [new VertexBoneWeight(HumanoidBone.Hips, 1f)];
                continue;
            }

            for (var i = 0; i < list.Count; i++)
                list[i] = new VertexBoneWeight(list[i].Bone, list[i].Weight / wSum);
            weights[v] = list.ToArray();
        }

        return new SkinnedHumanoidMesh(
            mesh,
            weights,
            SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind));
    }

    static bool IsCoreTorso(HumanoidBone bone) =>
        bone is HumanoidBone.Hips or HumanoidBone.Spine or HumanoidBone.Spine1;

    static bool IsLimbBone(HumanoidBone bone) =>
        bone is HumanoidBone.LeftUpLeg or HumanoidBone.LeftLeg or HumanoidBone.LeftFoot
            or HumanoidBone.RightUpLeg or HumanoidBone.RightLeg or HumanoidBone.RightFoot
            or HumanoidBone.LeftShoulder or HumanoidBone.LeftArm or HumanoidBone.LeftForeArm or HumanoidBone.LeftHand
            or HumanoidBone.RightShoulder or HumanoidBone.RightArm or HumanoidBone.RightForeArm or HumanoidBone.RightHand;

    static float MinLimbShaftDistSq(Vector3 p, HumanoidBindPose bind, Vector3[] joints)
    {
        var min = float.MaxValue;
        for (var b = 0; b < (int)HumanoidBone.Count; b++)
        {
            var bone = (HumanoidBone)b;
            if (!IsLimbBone(bone))
                continue;
            min = MathF.Min(min, DistanceToBoneSq(p, bind, bone, joints));
        }

        return min;
    }

    /// <summary>Distance² from point to the bone shaft (parent→joint), or to the joint if root.</summary>
    static float DistanceToBoneSq(Vector3 p, HumanoidBindPose bind, HumanoidBone bone, Vector3[] joints)
    {
        var end = joints[(int)bone];
        var parent = HumanoidHierarchy.ParentBone(bone);
        if (parent is null)
            return Vector3.DistanceSquared(p, end);

        var start = joints[(int)parent.Value];
        var ab = end - start;
        var lenSq = ab.LengthSquared();
        if (lenSq < 1e-10f)
            return Vector3.DistanceSquared(p, end);

        var t = System.Math.Clamp(Vector3.Dot(p - start, ab) / lenSq, 0f, 1f);
        var closest = start + ab * t;
        return Vector3.DistanceSquared(p, closest);
    }

    /// <summary>
    /// Maps Mixamo / Unity-style bone names onto <see cref="HumanoidBone"/>.
    /// Returns null when the name is not recognized.
    /// </summary>
    public static HumanoidBone? TryMapBoneName(string? boneName)
    {
        if (string.IsNullOrWhiteSpace(boneName))
            return null;

        var n = boneName.Trim();
        var colon = n.LastIndexOf(':');
        if (colon >= 0 && colon < n.Length - 1)
            n = n[(colon + 1)..];
        if (n.StartsWith("mixamorig", StringComparison.OrdinalIgnoreCase))
            n = n["mixamorig".Length..].TrimStart('_', ':');

        foreach (HumanoidBone bone in Enum.GetValues<HumanoidBone>())
        {
            if (bone == HumanoidBone.Count)
                continue;
            if (n.Equals(bone.ToString(), StringComparison.OrdinalIgnoreCase))
                return bone;
        }

        // Common aliases
        return n.ToLowerInvariant() switch
        {
            "hip" or "pelvis" => HumanoidBone.Hips,
            "chest" or "upperchest" => HumanoidBone.Spine2,
            "l_upleg" or "leftthigh" => HumanoidBone.LeftUpLeg,
            "r_upleg" or "rightthigh" => HumanoidBone.RightUpLeg,
            "l_leg" or "leftcalf" => HumanoidBone.LeftLeg,
            "r_leg" or "rightcalf" => HumanoidBone.RightLeg,
            "l_foot" => HumanoidBone.LeftFoot,
            "r_foot" => HumanoidBone.RightFoot,
            "l_arm" or "leftupperarm" => HumanoidBone.LeftArm,
            "r_arm" or "rightupperarm" => HumanoidBone.RightArm,
            "l_forearm" or "leftlowerarm" => HumanoidBone.LeftForeArm,
            "r_forearm" or "rightlowerarm" => HumanoidBone.RightForeArm,
            "l_hand" => HumanoidBone.LeftHand,
            "r_hand" => HumanoidBone.RightHand,
            _ => null,
        };
    }

    /// <summary>
    /// Builds a skinned mesh from author bone names (e.g. Assimp) when names map to <see cref="HumanoidBone"/>.
    /// Returns null if fewer than half of weighted influences map successfully.
    /// </summary>
    public static SkinnedHumanoidMesh? TryBindNamedWeights(
        TriangleMesh mesh,
        IReadOnlyList<NamedBoneWeight[]> vertexWeights,
        HumanoidBindPose bind)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(vertexWeights);
        ArgumentNullException.ThrowIfNull(bind);
        if (vertexWeights.Count != mesh.VertexCount)
            throw new ArgumentException("Weight list length must match vertex count.", nameof(vertexWeights));

        var mapped = new VertexBoneWeight[mesh.VertexCount][];
        var mappedInfluences = 0;
        var totalInfluences = 0;

        for (var v = 0; v < mesh.VertexCount; v++)
        {
            var src = vertexWeights[v] ?? [];
            var list = new List<VertexBoneWeight>(src.Length);
            var wSum = 0f;
            foreach (var nw in src)
            {
                totalInfluences++;
                var bone = TryMapBoneName(nw.BoneName);
                if (bone is null)
                    continue;
                mappedInfluences++;
                list.Add(new VertexBoneWeight(bone.Value, nw.Weight));
                wSum += nw.Weight;
            }

            if (list.Count == 0 || wSum < 1e-8f)
            {
                mapped[v] = [new VertexBoneWeight(HumanoidBone.Hips, 1f)];
                continue;
            }

            for (var i = 0; i < list.Count; i++)
                list[i] = new VertexBoneWeight(list[i].Bone, list[i].Weight / wSum);
            mapped[v] = list.ToArray();
        }

        if (totalInfluences > 0 && mappedInfluences < totalInfluences * 0.5f)
            return null;

        return new SkinnedHumanoidMesh(
            mesh,
            mapped,
            SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind));
    }
}

/// <summary>Bone influence keyed by authoring name (Assimp / Mixamo).</summary>
public readonly record struct NamedBoneWeight(string BoneName, float Weight);
