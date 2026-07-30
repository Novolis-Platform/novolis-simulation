using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Host-agnostic look deltas (radians / meters). Apps map mouse/gamepad here.</summary>
public readonly record struct LookIntent(float DeltaYaw, float DeltaPitch, float ZoomDelta = 0f);

/// <summary>
/// Host-agnostic locomotion wish. <see cref="WishDirection"/> is typically XZ-planar
/// (Y ignored by motors); length may exceed 1 before normalization.
/// </summary>
public readonly record struct MoveIntent(
    Vector3 WishDirection,
    bool Jump = false,
    bool Sprint = false);
