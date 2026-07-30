using System.Globalization;
using System.Numerics;
using System.Text;

namespace Novolis.Simulation.Humanoid.Import;

/// <summary>Parses Biovision Hierarchy (BVH) mocap into a <see cref="HumanoidAnimationClip"/>.</summary>
public static class BvhHumanoidImporter
{
    /// <summary>Loads BVH text (cm or mixed units) and retargets named channels onto <see cref="HumanoidBone"/>.</summary>
    /// <param name="bvhText">Full BVH document.</param>
    /// <param name="metersPerUnit">Scale applied to ROOT positions (default 0.01 for cm→m).</param>
    public static HumanoidAnimationClip Import(string bvhText, float metersPerUnit = 0.01f)
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

        var clip = new HumanoidAnimationClip("bvh") { Loop = true };
        var channelCount = joints.Sum(j => j.Channels.Count);
        for (var f = 0; f < frameCount && i < lines.Length; f++, i++)
        {
            var values = ParseFloats(lines[i]);
            if (values.Length < channelCount)
                throw new FormatException($"BVH frame {f} has {values.Length} values, expected {channelCount}.");

            var key = new HumanoidKeyframe { TimeSeconds = f * frameTime };
            var cursor = 0;
            Vector3? root = null;
            foreach (var joint in joints)
            {
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

                if (!HumanoidBoneNames.TryResolve(joint.Name, out var bone))
                    continue;

                // BVH typically ZYX intrinsic degrees.
                var q =
                    Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Deg(rz)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitY, Deg(ry)) *
                    Quaternion.CreateFromAxisAngle(Vector3.UnitX, Deg(rx));
                key.LocalRotations[bone] = q;
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

        return clip;
    }

    /// <summary>Loads BVH from a UTF-8 file path.</summary>
    public static HumanoidAnimationClip ImportFile(string path, float metersPerUnit = 0.01f) =>
        Import(File.ReadAllText(path, Encoding.UTF8), metersPerUnit);

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
        public List<string> Channels { get; } = [];
        public List<BvhJoint> Children { get; } = [];
    }
}
