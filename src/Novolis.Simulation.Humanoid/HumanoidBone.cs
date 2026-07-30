namespace Novolis.Simulation.Humanoid;

/// <summary>
/// Canonical biped bones (Mixamo / Unity Humanoid naming).
/// Index order is stable for packed pose arrays; do not reorder without a format bump.
/// </summary>
public enum HumanoidBone
{
    /// <summary>Root / hips.</summary>
    Hips = 0,

    /// <summary>Lower spine.</summary>
    Spine,

    /// <summary>Mid spine.</summary>
    Spine1,

    /// <summary>Upper spine / chest.</summary>
    Spine2,

    /// <summary>Neck.</summary>
    Neck,

    /// <summary>Head.</summary>
    Head,

    /// <summary>Left upper leg.</summary>
    LeftUpLeg,

    /// <summary>Left lower leg.</summary>
    LeftLeg,

    /// <summary>Left foot.</summary>
    LeftFoot,

    /// <summary>Left toe.</summary>
    LeftToeBase,

    /// <summary>Right upper leg.</summary>
    RightUpLeg,

    /// <summary>Right lower leg.</summary>
    RightLeg,

    /// <summary>Right foot.</summary>
    RightFoot,

    /// <summary>Right toe.</summary>
    RightToeBase,

    /// <summary>Left clavicle / shoulder girdle.</summary>
    LeftShoulder,

    /// <summary>Left upper arm.</summary>
    LeftArm,

    /// <summary>Left forearm.</summary>
    LeftForeArm,

    /// <summary>Left hand.</summary>
    LeftHand,

    /// <summary>Right clavicle / shoulder girdle.</summary>
    RightShoulder,

    /// <summary>Right upper arm.</summary>
    RightArm,

    /// <summary>Right forearm.</summary>
    RightForeArm,

    /// <summary>Right hand.</summary>
    RightHand,

    /// <summary>Number of standard bones (not a bone).</summary>
    Count,
}
