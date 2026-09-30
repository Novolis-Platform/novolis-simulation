using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>
/// Host-agnostic locomotion wish. <see cref="WishDirection"/> is typically XZ-planar
/// (Y ignored by motors); length may exceed 1 before normalization.
/// </summary>
public readonly record struct MoveIntent(
    Vector3 WishDirection,
    bool Jump = false,
    bool Sprint = false);
