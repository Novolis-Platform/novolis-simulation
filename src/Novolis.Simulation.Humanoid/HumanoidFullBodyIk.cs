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

/// <summary>
/// Multi-effector IK: feet then hands via <see cref="TwoBoneIk"/>; optional head via spine
/// <see cref="HumanoidChainIk"/>. Does not invent ground contacts — callers supply foot targets.
/// Hip root is left unchanged unless a future option opts in.
/// </summary>
public static class HumanoidFullBodyIk
{
    private static readonly HumanoidBone[] SpineToHead =
    [
        HumanoidBone.Spine,
        HumanoidBone.Spine1,
        HumanoidBone.Spine2,
        HumanoidBone.Neck,
        HumanoidBone.Head,
    ];

    /// <summary>Applies configured effectors to <paramref name="world"/> in place.</summary>
    public static void Apply(
        HumanoidWorldPose world,
        HumanoidBindPose bind,
        in HumanoidFullBodyIkTargets targets)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(bind);

        var poles = targets;
        if (poles.LeftHandPole == default) poles.LeftHandPole = Vector3.UnitZ;
        if (poles.RightHandPole == default) poles.RightHandPole = Vector3.UnitZ;
        if (poles.LeftFootPole == default) poles.LeftFootPole = Vector3.UnitZ;
        if (poles.RightFootPole == default) poles.RightFootPole = Vector3.UnitZ;

        // Lower body first so upper-body IK sees updated hip-relative shoulders.
        if (poles.LeftFoot is { } leftFoot)
        {
            ApplyLeg(
                world, bind,
                HumanoidBone.LeftUpLeg, HumanoidBone.LeftLeg, HumanoidBone.LeftFoot,
                leftFoot, poles.LeftFootPole);
        }

        if (poles.RightFoot is { } rightFoot)
        {
            ApplyLeg(
                world, bind,
                HumanoidBone.RightUpLeg, HumanoidBone.RightLeg, HumanoidBone.RightFoot,
                rightFoot, poles.RightFootPole);
        }

        if (poles.Head is { } head)
            HumanoidChainIk.Apply(world, bind, SpineToHead, head, pinRoot: true);

        if (poles.LeftHand is { } leftHand)
        {
            ApplyArm(
                world, bind,
                HumanoidBone.LeftArm, HumanoidBone.LeftForeArm, HumanoidBone.LeftHand,
                leftHand, poles.LeftHandPole);
        }

        if (poles.RightHand is { } rightHand)
        {
            ApplyArm(
                world, bind,
                HumanoidBone.RightArm, HumanoidBone.RightForeArm, HumanoidBone.RightHand,
                rightHand, poles.RightHandPole);
        }
    }

    private static void ApplyArm(
        HumanoidWorldPose world,
        HumanoidBindPose bind,
        HumanoidBone root,
        HumanoidBone mid,
        HumanoidBone end,
        Vector3 target,
        Vector3 pole)
    {
        var upper = Vector3.Distance(bind[root], bind[mid]);
        var lower = Vector3.Distance(bind[mid], bind[end]);
        TwoBoneIk.ApplyLimb(world, bind, root, mid, end, target, upper, lower, pole);
    }

    private static void ApplyLeg(
        HumanoidWorldPose world,
        HumanoidBindPose bind,
        HumanoidBone root,
        HumanoidBone mid,
        HumanoidBone end,
        Vector3 target,
        Vector3 pole) =>
        ApplyArm(world, bind, root, mid, end, target, pole);
}
