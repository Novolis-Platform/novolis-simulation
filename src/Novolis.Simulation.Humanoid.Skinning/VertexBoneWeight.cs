using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

/// <summary>One bone influence on a vertex (weight should sum to ~1 across influences).</summary>
public readonly record struct VertexBoneWeight(HumanoidBone Bone, float Weight);
