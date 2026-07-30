using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>Animated pose: root translation plus local rotations (parent space) for each bone.</summary>
public sealed class HumanoidPose
{
    private readonly Quaternion[] _localRotations = new Quaternion[(int)HumanoidBone.Count];

    /// <summary>Creates an identity pose (all local rotations identity).</summary>
    public HumanoidPose()
    {
        for (var i = 0; i < _localRotations.Length; i++)
            _localRotations[i] = Quaternion.Identity;
    }

    /// <summary>World translation of the hips / root.</summary>
    public Vector3 RootTranslation { get; set; }

    /// <summary>Gets or sets a local rotation.</summary>
    public Quaternion this[HumanoidBone bone]
    {
        get => _localRotations[(int)bone];
        set => _localRotations[(int)bone] = Quaternion.Normalize(value);
    }

    /// <summary>Copies local rotations into <paramref name="destination"/> (must be length Count).</summary>
    public void CopyLocalRotationsTo(Span<Quaternion> destination)
    {
        if (destination.Length < _localRotations.Length)
            throw new ArgumentException("Destination too short.", nameof(destination));
        _localRotations.AsSpan().CopyTo(destination);
    }

    /// <summary>Resets all local rotations to identity.</summary>
    public void ResetToBindLocals()
    {
        for (var i = 0; i < _localRotations.Length; i++)
            _localRotations[i] = Quaternion.Identity;
    }

    /// <summary>Creates a pose at bind with optional root offset.</summary>
    public static HumanoidPose FromBind(HumanoidBindPose bind, Vector3? rootOverride = null)
    {
        var pose = new HumanoidPose
        {
            RootTranslation = rootOverride ?? bind[HumanoidBone.Hips],
        };
        return pose;
    }
}
