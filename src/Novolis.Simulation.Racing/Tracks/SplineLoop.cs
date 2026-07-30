namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Closed.</summary>
/// <summary>ControlPoints.</summary>
/// <summary>SplineLoop operation.</summary>
/// <summary>Represents SplineLoop.</summary>
public sealed record SplineLoop(IReadOnlyList<Vector3> ControlPoints, bool Closed = true);
