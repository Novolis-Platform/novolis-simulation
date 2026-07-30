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

/// <summary>Simple skeletal animation clip for retargeted mocap / authored motion.</summary>
public sealed class HumanoidAnimationClip
{
    private readonly List<HumanoidKeyframe> _keys = [];

    /// <summary>Clip display name.</summary>
    public string Name { get; }

    /// <summary>When true, sample wraps with modulo duration.</summary>
    public bool Loop { get; set; } = true;

    /// <summary>Ordered keyframes.</summary>
    public IReadOnlyList<HumanoidKeyframe> Keys => _keys;

    /// <summary>Creates a named clip.</summary>
    public HumanoidAnimationClip(string name) => Name = name;

    /// <summary>Adds a keyframe (keeps keys sorted by time).</summary>
    public HumanoidAnimationClip AddKey(HumanoidKeyframe key)
    {
        _keys.Add(key);
        _keys.Sort(static (a, b) => a.TimeSeconds.CompareTo(b.TimeSeconds));
        return this;
    }

    /// <summary>Duration of the last key (0 if empty).</summary>
    public float DurationSeconds => _keys.Count == 0 ? 0f : _keys[^1].TimeSeconds;

    /// <summary>Samples into <paramref name="pose"/> at <paramref name="timeSeconds"/>.</summary>
    public void Sample(float timeSeconds, HumanoidPose pose, HumanoidBindPose? bindForDefaultRoot = null)
    {
        if (_keys.Count == 0)
        {
            pose.ResetToBindLocals();
            if (bindForDefaultRoot is not null)
                pose.RootTranslation = bindForDefaultRoot[HumanoidBone.Hips];
            return;
        }

        var t = timeSeconds;
        var duration = DurationSeconds;
        if (Loop && duration > 1e-5f)
        {
            t %= duration;
            if (t < 0f)
                t += duration;
        }
        else
        {
            t = Math.Clamp(t, 0f, duration);
        }

        if (_keys.Count == 1 || t <= _keys[0].TimeSeconds)
        {
            Apply(_keys[0], pose, bindForDefaultRoot);
            return;
        }

        if (t >= _keys[^1].TimeSeconds)
        {
            Apply(_keys[^1], pose, bindForDefaultRoot);
            return;
        }

        var i = 0;
        while (i + 1 < _keys.Count && _keys[i + 1].TimeSeconds < t)
            i++;

        var a = _keys[i];
        var b = _keys[i + 1];
        var span = b.TimeSeconds - a.TimeSeconds;
        var u = span < 1e-5f ? 0f : (t - a.TimeSeconds) / span;

        pose.ResetToBindLocals();
        for (var bone = 0; bone < (int)HumanoidBone.Count; bone++)
        {
            var id = (HumanoidBone)bone;
            var qa = a.LocalRotations.GetValueOrDefault(id, Quaternion.Identity);
            var qb = b.LocalRotations.GetValueOrDefault(id, Quaternion.Identity);
            pose[id] = Quaternion.Slerp(qa, qb, u);
        }

        var ra = a.RootTranslation ?? bindForDefaultRoot?[HumanoidBone.Hips] ?? pose.RootTranslation;
        var rb = b.RootTranslation ?? ra;
        pose.RootTranslation = Vector3.Lerp(ra, rb, u);
    }

    private static void Apply(HumanoidKeyframe key, HumanoidPose pose, HumanoidBindPose? bind)
    {
        pose.ResetToBindLocals();
        foreach (var (bone, rot) in key.LocalRotations)
            pose[bone] = rot;
        if (key.RootTranslation is { } root)
            pose.RootTranslation = root;
        else if (bind is not null)
            pose.RootTranslation = bind[HumanoidBone.Hips];
    }
}
