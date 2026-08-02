using System.Globalization;
using System.Numerics;
using System.Text;

namespace Novolis.Simulation.Humanoid.Import;

/// <summary>Parses Biovision Hierarchy (BVH) mocap into a <see cref="HumanoidAnimationClip"/>.</summary>
public static class BvhHumanoidImporter
{
    /// <summary>Loads BVH text and retargets named channels onto <see cref="HumanoidBone"/>.</summary>
    public static HumanoidAnimationClip Import(
        string bvhText,
        float metersPerUnit = 0.01f,
        Func<string, string>? renameJoint = null) =>
        ImportWithBind(bvhText, metersPerUnit, renameJoint).Clip;

    /// <summary>
    /// Imports clip plus a rest-pose <see cref="HumanoidBindPose"/> built from BVH OFFSETS
    /// (scaled to <paramref name="targetHeightMeters"/>). Use that bind for FK so CMU limb
    /// proportions match the captured skeleton instead of Mixamo defaults.
    /// </summary>
    public static (HumanoidAnimationClip Clip, HumanoidBindPose Bind) ImportWithBind(
        string bvhText,
        float metersPerUnit = 0.01f,
        Func<string, string>? renameJoint = null,
        float targetHeightMeters = 1.72f)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bvhText);
        var lines = bvhText.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var i = 0;
        if (!lines[i].Equals("HIERARCHY", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("BVH must start with HIERARCHY.");
        i++;

        var joints = new List<BvhJoint>();
        ParseHierarchy(lines, ref i, parent: null, joints);

        if (i >= lines.Length || !lines[i].Equals("MOTION", StringComparison.OrdinalIgnoreCase))
            throw new FormatException("BVH missing MOTION section.");
        i++;

        var frameCount = 0;
        var frameTime = 1f / 30f;
        while (i < lines.Length)
        {
            var line = lines[i];
            if (line.StartsWith("Frames:", StringComparison.OrdinalIgnoreCase))
            {
                frameCount = int.Parse(line.AsSpan(7).Trim(), CultureInfo.InvariantCulture);
                i++;
            }
            else if (line.StartsWith("Frame Time:", StringComparison.OrdinalIgnoreCase))
            {
                frameTime = float.Parse(line.AsSpan(11).Trim(), CultureInfo.InvariantCulture);
                i++;
                break;
            }
            else
            {
                i++;
            }
        }

        var bind = BuildRestBind(joints, metersPerUnit, renameJoint, targetHeightMeters);
        var clip = new HumanoidAnimationClip("bvh") { Loop = true };
        var channelCount = joints.Sum(j => j.Channels.Count);
        var foldInto = BuildFoldMap(joints, renameJoint);

        for (var f = 0; f < frameCount && i < lines.Length; f++, i++)
        {
            var values = ParseFloats(lines[i]);
            if (values.Length < channelCount)
                throw new FormatException($"BVH frame {f} has {values.Length} values, expected {channelCount}.");

            var key = new HumanoidKeyframe { TimeSeconds = f * frameTime };
            var cursor = 0;
            Vector3? root = null;
            var jointRotation = new Quaternion[joints.Count];
            for (var ji = 0; ji < joints.Count; ji++)
            {
                var joint = joints[ji];
                var tx = 0f;
                var ty = 0f;
                var tz = 0f;
                var rx = 0f;
                var ry = 0f;
                var rz = 0f;
                foreach (var ch in joint.Channels)
                {
                    var v = values[cursor++];
                    switch (ch)
                    {
                        case "Xposition": tx = v; break;
                        case "Yposition": ty = v; break;
                        case "Zposition": tz = v; break;
                        case "Xrotation": rx = v; break;
                        case "Yrotation": ry = v; break;
                        case "Zrotation": rz = v; break;
                    }
                }

                if (joint.IsRoot)
                    root = new Vector3(tx, ty, tz) * metersPerUnit;

                jointRotation[ji] =
                    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Deg(rz)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, Deg(ry)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitX, Deg(rx));
            }

            for (var ji = 0; ji < joints.Count; ji++)
            {
                var joint = joints[ji];
                var resolveName = renameJoint is null ? joint.Name : renameJoint(joint.Name);
                if (!HumanoidBoneNames.TryResolve(resolveName, out var bone))
                    continue;

                var q = jointRotation[ji];
                if (foldInto[ji] is { Count: > 0 } ancestors)
                {
                    for (var a = 0; a < ancestors.Count; a++)
                        q = jointRotation[ancestors[a]] * q;
                }

                key.LocalRotations[bone] = Quaternion.Normalize(q);
            }

            if (root is { } r)
                key = new HumanoidKeyframe
                {
                    TimeSeconds = key.TimeSeconds,
                    RootTranslation = r,
                    LocalRotations = key.LocalRotations,
                };
            clip.AddKey(key);
        }

        return (clip, bind);
    }

    /// <summary>Loads BVH from a UTF-8 file path.</summary>
    public static HumanoidAnimationClip ImportFile(
        string path,
        float metersPerUnit = 0.01f,
        Func<string, string>? renameJoint = null) =>
        Import(File.ReadAllText(path, Encoding.UTF8), metersPerUnit, renameJoint);

    /// <summary>Loads BVH file and rest bind (see <see cref="ImportWithBind"/>).</summary>
    public static (HumanoidAnimationClip Clip, HumanoidBindPose Bind) ImportFileWithBind(
        string path,
        float metersPerUnit = 0.01f,
        Func<string, string>? renameJoint = null,
        float targetHeightMeters = 1.72f) =>
        ImportWithBind(File.ReadAllText(path, Encoding.UTF8), metersPerUnit, renameJoint, targetHeightMeters);

    /// <summary>
    /// CMU Graphics Lab BVH joint names → Mixamo-style names for <see cref="HumanoidBoneNames"/>.
    /// Dummy <c>LHipJoint</c>/<c>RHipJoint</c> stay unmapped and fold into Left/RightUpLeg.
    /// </summary>
    public static string RenameCmuJoint(string name) => name switch
    {
        "LowerBack" => "Spine",
        "Spine" => "Spine1",
        "Spine1" => "Spine2",
        "Neck1" => "Head",
        _ => name,
    };

    /// <summary>
    /// Rest-pose FK with identity rotations using BVH OFFSETs, then uniform-scale to
    /// <paramref name="targetHeightMeters"/> with feet near y=0.
    /// </summary>
    private static HumanoidBindPose BuildRestBind(
        List<BvhJoint> joints,
        float metersPerUnit,
        Func<string, string>? renameJoint,
        float targetHeightMeters)
    {
        var indexOf = new Dictionary<BvhJoint, int>();
        for (var i = 0; i < joints.Count; i++)
            indexOf[joints[i]] = i;

        var parentOf = new int[joints.Count];
        Array.Fill(parentOf, -1);
        for (var i = 0; i < joints.Count; i++)
        {
            foreach (var child in joints[i].Children)
                parentOf[indexOf[child]] = i;
        }

        // Rest world = accumulate OFFSET along parents (identity rotation).
        var rest = new Vector3[joints.Count];
        for (var ji = 0; ji < joints.Count; ji++)
        {
            var p = joints[ji].Offset * metersPerUnit;
            var parent = parentOf[ji];
            rest[ji] = parent < 0 ? p : rest[parent] + p;
        }

        var positions = new Vector3[(int)HumanoidBone.Count];
        var present = new bool[(int)HumanoidBone.Count];
        for (var ji = 0; ji < joints.Count; ji++)
        {
            var resolveName = renameJoint is null ? joints[ji].Name : renameJoint(joints[ji].Name);
            if (!HumanoidBoneNames.TryResolve(resolveName, out var bone))
                continue;
            positions[(int)bone] = rest[ji];
            present[(int)bone] = true;
        }

        if (!present[(int)HumanoidBone.Hips])
            return HumanoidBindPose.CreateDefaultTPose(targetHeightMeters);

        // Scale so head–foot span ≈ target height; plant feet near ground; center hips XZ.
        var hips = positions[(int)HumanoidBone.Hips];
        var minY = float.MaxValue;
        var maxY = float.MinValue;
        for (var b = 0; b < present.Length; b++)
        {
            if (!present[b]) continue;
            minY = MathF.Min(minY, positions[b].Y);
            maxY = MathF.Max(maxY, positions[b].Y);
        }

        var span = MathF.Max(1e-3f, maxY - minY);
        var scale = targetHeightMeters / span;
        for (var b = 0; b < positions.Length; b++)
        {
            if (!present[b]) continue;
            var p = (positions[b] - hips) * scale;
            positions[b] = p;
        }

        // After centering on hips, feet are negative Y — lift so lowest foot ≈ 0.02 m.
        minY = float.MaxValue;
        for (var b = 0; b < present.Length; b++)
        {
            if (!present[b]) continue;
            minY = MathF.Min(minY, positions[b].Y);
        }

        var lift = 0.02f - minY;
        for (var b = 0; b < positions.Length; b++)
        {
            if (!present[b]) continue;
            positions[b] += new Vector3(0f, lift, 0f);
        }

        return HumanoidBindPose.FromWorldPositions(positions, present, targetHeightMeters);
    }

    private static List<int>?[] BuildFoldMap(List<BvhJoint> joints, Func<string, string>? renameJoint)
    {
        var indexOf = new Dictionary<BvhJoint, int>();
        for (var i = 0; i < joints.Count; i++)
            indexOf[joints[i]] = i;

        var parentOf = new int[joints.Count];
        Array.Fill(parentOf, -1);
        for (var i = 0; i < joints.Count; i++)
        {
            foreach (var child in joints[i].Children)
                parentOf[indexOf[child]] = i;
        }

        bool IsMapped(int ji)
        {
            var name = renameJoint is null ? joints[ji].Name : renameJoint(joints[ji].Name);
            return HumanoidBoneNames.TryResolve(name, out _);
        }

        var fold = new List<int>?[joints.Count];
        for (var ji = 0; ji < joints.Count; ji++)
        {
            if (!IsMapped(ji))
                continue;

            List<int>? chain = null;
            for (var p = parentOf[ji]; p >= 0 && !IsMapped(p); p = parentOf[p])
            {
                chain ??= [];
                chain.Add(p);
            }

            if (chain is { Count: > 0 })
            {
                chain.Reverse();
                fold[ji] = chain;
            }
        }

        return fold;
    }

    private static float Deg(float degrees) => degrees * (MathF.PI / 180f);

    private static float[] ParseFloats(string line)
    {
        var parts = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var values = new float[parts.Length];
        for (var i = 0; i < parts.Length; i++)
            values[i] = float.Parse(parts[i], CultureInfo.InvariantCulture);
        return values;
    }

    private static void ParseHierarchy(string[] lines, ref int i, BvhJoint? parent, List<BvhJoint> joints)
    {
        while (i < lines.Length)
        {
            var line = lines[i];
            if (line.Equals("MOTION", StringComparison.OrdinalIgnoreCase))
                return;

            if (line.StartsWith("ROOT ", StringComparison.OrdinalIgnoreCase) ||
                line.StartsWith("JOINT ", StringComparison.OrdinalIgnoreCase))
            {
                var isRoot = line.StartsWith("ROOT ", StringComparison.OrdinalIgnoreCase);
                var name = line[(isRoot ? 5 : 6)..].Trim();
                i++;
                Expect(lines, ref i, "{");
                var joint = new BvhJoint(name, isRoot);
                joints.Add(joint);
                parent?.Children.Add(joint);

                while (i < lines.Length && !lines[i].StartsWith("}", StringComparison.Ordinal))
                {
                    if (lines[i].StartsWith("OFFSET", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = lines[i].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                        if (parts.Length >= 4)
                        {
                            joint.Offset = new Vector3(
                                float.Parse(parts[1], CultureInfo.InvariantCulture),
                                float.Parse(parts[2], CultureInfo.InvariantCulture),
                                float.Parse(parts[3], CultureInfo.InvariantCulture));
                        }

                        i++;
                    }
                    else if (lines[i].StartsWith("CHANNELS", StringComparison.OrdinalIgnoreCase))
                    {
                        var parts = lines[i].Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
                        var count = int.Parse(parts[1], CultureInfo.InvariantCulture);
                        for (var c = 0; c < count; c++)
                            joint.Channels.Add(parts[2 + c]);
                        i++;
                    }
                    else if (lines[i].StartsWith("JOINT ", StringComparison.OrdinalIgnoreCase) ||
                             lines[i].StartsWith("ROOT ", StringComparison.OrdinalIgnoreCase))
                    {
                        ParseHierarchy(lines, ref i, joint, joints);
                    }
                    else if (lines[i].StartsWith("End Site", StringComparison.OrdinalIgnoreCase))
                    {
                        i++;
                        Expect(lines, ref i, "{");
                        while (i < lines.Length && !lines[i].StartsWith("}", StringComparison.Ordinal))
                            i++;
                        Expect(lines, ref i, "}");
                    }
                    else
                    {
                        i++;
                    }
                }

                Expect(lines, ref i, "}");
                return;
            }
            else
            {
                i++;
            }
        }
    }

    private static void Expect(string[] lines, ref int i, string token)
    {
        if (i >= lines.Length || !lines[i].StartsWith(token, StringComparison.Ordinal))
            throw new FormatException($"Expected '{token}' at line {i + 1}.");
        i++;
    }

    private sealed class BvhJoint(string name, bool isRoot)
    {
        public string Name { get; } = name;
        public bool IsRoot { get; } = isRoot;
        public Vector3 Offset { get; set; }
        public List<string> Channels { get; } = [];
        public List<BvhJoint> Children { get; } = [];
    }
}
