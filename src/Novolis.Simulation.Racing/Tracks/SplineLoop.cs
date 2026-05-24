namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed record SplineLoop(IReadOnlyList<Vector2> ControlPoints, bool Closed = true);
