namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed record TrackGate(int Index, Vector2 A, Vector2 B, double SampleT);
