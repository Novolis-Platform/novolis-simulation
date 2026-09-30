using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>Optional end-effector targets for <see cref="HumanoidFullBodyIk"/>.</summary>
public struct HumanoidFullBodyIkTargets
{
    /// <summary>Left hand world target.</summary>
    public Vector3? LeftHand { get; set; }

    /// <summary>Right hand world target.</summary>
    public Vector3? RightHand { get; set; }

    /// <summary>Left foot world target.</summary>
    public Vector3? LeftFoot { get; set; }

    /// <summary>Right foot world target.</summary>
    public Vector3? RightFoot { get; set; }

    /// <summary>Head world target (spine FABRIK from <see cref="HumanoidBone.Spine"/>).</summary>
    public Vector3? Head { get; set; }

    /// <summary>Pole for left arm bend (default +Z).</summary>
    public Vector3 LeftHandPole { get; set; }

    /// <summary>Pole for right arm bend.</summary>
    public Vector3 RightHandPole { get; set; }

    /// <summary>Pole for left leg bend.</summary>
    public Vector3 LeftFootPole { get; set; }

    /// <summary>Pole for right leg bend.</summary>
    public Vector3 RightFootPole { get; set; }

    /// <summary>Creates targets with default poles (+Z arms, +Z legs).</summary>
    public static HumanoidFullBodyIkTargets WithDefaults() => new()
    {
        LeftHandPole = Vector3.UnitZ,
        RightHandPole = Vector3.UnitZ,
        LeftFootPole = Vector3.UnitZ,
        RightFootPole = Vector3.UnitZ,
    };
}
