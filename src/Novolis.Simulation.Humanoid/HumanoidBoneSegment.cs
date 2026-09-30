using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>Debug / stick-figure segment between two bones.</summary>
public readonly record struct HumanoidBoneSegment(HumanoidBone From, HumanoidBone To, Vector3 Start, Vector3 End);
