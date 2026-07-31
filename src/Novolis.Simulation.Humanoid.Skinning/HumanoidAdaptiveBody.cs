using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

/// <summary>
/// Builds an <see cref="AdaptiveMesh"/> person hull over the 11-sphere ragdoll topology
/// (<see cref="HumanoidRagdollMap"/>), so one mesh follows the ragdoll as a single body.
/// </summary>
public static class HumanoidAdaptiveBody
{
    /// <summary>Ragdoll sphere count expected by <see cref="CreateFromRagdollBind"/>.</summary>
    public const int SphereCount = 11;

    private static readonly (int A, int B)[] Edges =
    [
        (HumanoidRagdollMap.RagdollHip, HumanoidRagdollMap.RagdollChest),
        (HumanoidRagdollMap.RagdollChest, HumanoidRagdollMap.RagdollHead),
        (HumanoidRagdollMap.RagdollHip, HumanoidRagdollMap.RagdollLeftKnee),
        (HumanoidRagdollMap.RagdollLeftKnee, HumanoidRagdollMap.RagdollLeftFoot),
        (HumanoidRagdollMap.RagdollHip, HumanoidRagdollMap.RagdollRightKnee),
        (HumanoidRagdollMap.RagdollRightKnee, HumanoidRagdollMap.RagdollRightFoot),
        (HumanoidRagdollMap.RagdollChest, HumanoidRagdollMap.RagdollLeftShoulder),
        (HumanoidRagdollMap.RagdollLeftShoulder, HumanoidRagdollMap.RagdollLeftHand),
        (HumanoidRagdollMap.RagdollChest, HumanoidRagdollMap.RagdollRightShoulder),
        (HumanoidRagdollMap.RagdollRightShoulder, HumanoidRagdollMap.RagdollRightHand),
    ];

    private static readonly float[] JointRadii =
    [
        0.12f, // hip
        0.09f, // l knee
        0.09f, // r knee
        0.11f, // chest
        0.14f, // head
        0.08f, // l shoulder
        0.08f, // r shoulder
        0.06f, // l hand
        0.06f, // r hand
        0.07f, // l foot
        0.07f, // r foot
    ];

    /// <summary>
    /// Creates a bind adaptive person mesh from ragdoll sphere centers (typically standing pose).
    /// Visual radii are mannequin-sized (not the physics collision radius).
    /// </summary>
    public static AdaptiveMesh CreateFromRagdollBind(ReadOnlySpan<Vector3> sphereCenters)
    {
        if (sphereCenters.Length < SphereCount)
            throw new ArgumentException($"Expected at least {SphereCount} sphere centers.", nameof(sphereCenters));

        var handles = new AdaptiveMeshHandle[SphereCount];
        for (var i = 0; i < SphereCount; i++)
            handles[i] = new AdaptiveMeshHandle(sphereCenters[i], JointRadii[i]);

        return AdaptiveMeshFactory.FromCapsuleGraph(handles, Edges, radialSegments: 6, ringsPerEdge: 2);
    }

    /// <summary>Copies current sphere centers into a handle-position buffer for AdaptiveMesh.Adapt.</summary>
    public static void CopySphereCenters(ReadOnlySpan<Vector3> sphereCenters, Span<Vector3> handlePositions)
    {
        if (sphereCenters.Length < handlePositions.Length)
            throw new ArgumentException("Not enough sphere centers.", nameof(sphereCenters));
        sphereCenters[..handlePositions.Length].CopyTo(handlePositions);
    }
}
