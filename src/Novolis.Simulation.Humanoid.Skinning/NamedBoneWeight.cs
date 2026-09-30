using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

/// <summary>Bone influence keyed by authoring name (Assimp / Mixamo).</summary>
public readonly record struct NamedBoneWeight(string BoneName, float Weight);
