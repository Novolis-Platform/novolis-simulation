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

/// <summary>Forward-kinematics from bind + local pose.</summary>
public static class HumanoidPoseSolver
{
    /// <summary>
    /// Computes world positions/rotations. Identity local rotations reproduce the bind T-pose
    /// translated so hips sit at <see cref="HumanoidPose.RootTranslation"/>.
    /// </summary>
    public static HumanoidWorldPose SolveWorld(HumanoidBindPose bind, HumanoidPose pose)
    {
        var world = new HumanoidWorldPose();

        for (var i = 0; i < (int)HumanoidBone.Count; i++)
        {
            var bone = (HumanoidBone)i;
            var parentIdx = HumanoidHierarchy.Parent(bone);
            if (parentIdx < 0)
            {
                world.Set(bone, pose.RootTranslation, pose[bone]);
                continue;
            }

            var parent = (HumanoidBone)parentIdx;
            var parentPos = world.Position(parent);
            var parentRot = world.Rotation(parent);
            var bindOffset = bind[bone] - bind[parent];
            var worldOffset = Vector3.Transform(bindOffset, parentRot);
            var childRot = Quaternion.Normalize(parentRot * pose[bone]);
            world.Set(bone, parentPos + worldOffset, childRot);
        }

        return world;
    }

    /// <summary>Returns world positions only (allocates).</summary>
    public static Vector3[] SolvePositions(HumanoidBindPose bind, HumanoidPose pose)
    {
        var world = SolveWorld(bind, pose);
        var copy = new Vector3[(int)HumanoidBone.Count];
        world.Positions.CopyTo(copy);
        return copy;
    }
}
