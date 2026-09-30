using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>Solved world transforms for one humanoid frame.</summary>
public sealed class HumanoidWorldPose
{
    private readonly Vector3[] _positions = new Vector3[(int)HumanoidBone.Count];
    private readonly Quaternion[] _rotations = new Quaternion[(int)HumanoidBone.Count];

    /// <summary>World positions.</summary>
    public ReadOnlySpan<Vector3> Positions => _positions;

    /// <summary>World rotations.</summary>
    public ReadOnlySpan<Quaternion> Rotations => _rotations;

    /// <summary>World position of a bone.</summary>
    public Vector3 Position(HumanoidBone bone) => _positions[(int)bone];

    /// <summary>World rotation of a bone.</summary>
    public Quaternion Rotation(HumanoidBone bone) => _rotations[(int)bone];

    /// <summary>Writes a solved bone transform (used by FK/IK).</summary>
    public void Set(HumanoidBone bone, Vector3 position, Quaternion rotation)
    {
        _positions[(int)bone] = position;
        _rotations[(int)bone] = rotation;
    }
}
