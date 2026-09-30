using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Host-agnostic look deltas (radians / meters). Apps map mouse/gamepad here.</summary>
public readonly record struct LookIntent(float DeltaYaw, float DeltaPitch, float ZoomDelta = 0f);
