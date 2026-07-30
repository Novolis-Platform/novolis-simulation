using System.Numerics;
using System.Text.Json;

namespace Novolis.Simulation.Humanoid.Import;

/// <summary>
/// Lightweight glTF 2.0 joint reader: maps node names onto <see cref="HumanoidBone"/> and
/// builds a single-frame bind clip (no accessor animation sampling in v1).
/// </summary>
public static class GltfHumanoidImporter
{
    /// <summary>Imports named nodes from a glTF JSON document into a one-frame clip at bind.</summary>
    public static HumanoidAnimationClip ImportBindPose(string gltfJson, HumanoidBindPose? fallbackBind = null)
    {
        using var doc = JsonDocument.Parse(gltfJson);
        var root = doc.RootElement;
        if (!root.TryGetProperty("nodes", out var nodesEl) || nodesEl.ValueKind != JsonValueKind.Array)
            throw new FormatException("glTF document has no nodes array.");

        var key = new HumanoidKeyframe { TimeSeconds = 0f };
        var bind = fallbackBind ?? HumanoidBindPose.CreateDefaultTPose();
        key.RootTranslation = bind[HumanoidBone.Hips];

        var index = 0;
        foreach (var node in nodesEl.EnumerateArray())
        {
            var name = node.TryGetProperty("name", out var n) ? n.GetString() ?? $"node_{index}" : $"node_{index}";
            index++;
            if (!HumanoidBoneNames.TryResolve(name, out var bone))
                continue;

            if (node.TryGetProperty("rotation", out var rot) && rot.ValueKind == JsonValueKind.Array)
            {
                var arr = rot.EnumerateArray().Select(e => e.GetSingle()).ToArray();
                if (arr.Length >= 4)
                    key.LocalRotations[bone] = Quaternion.Normalize(new Quaternion(arr[0], arr[1], arr[2], arr[3]));
            }

            if (bone == HumanoidBone.Hips && node.TryGetProperty("translation", out var tr) &&
                tr.ValueKind == JsonValueKind.Array)
            {
                var t = tr.EnumerateArray().Select(e => e.GetSingle()).ToArray();
                if (t.Length >= 3)
                    key.RootTranslation = new Vector3(t[0], t[1], t[2]);
            }
        }

        return new HumanoidAnimationClip("gltf-bind").AddKey(key);
    }

    /// <summary>Loads glTF JSON from a file path.</summary>
    public static HumanoidAnimationClip ImportBindPoseFile(string path, HumanoidBindPose? fallbackBind = null) =>
        ImportBindPose(File.ReadAllText(path), fallbackBind);
}
