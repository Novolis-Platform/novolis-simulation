namespace Novolis.Simulation.Racing.Cars;

using System.Numerics;

using Novolis.Simulation.Racing.Tracks;

/// <summary>Represents LapScorer.</summary>
public sealed class LapScorer : ILapScorer
{
    /// <summary>Update operation.</summary>
    public void Update(RaceTrack track, CarState car)
    {
        int numGates = track.Gates.Count;
        if (numGates == 0) return;

        int expectedGate = (car.CurrentGateIndex + 1) % numGates;
        var gate = track.Gates[expectedGate];

        if (!SegmentsIntersect(car.PreviousPosition, car.Position, gate.A, gate.B))
            return;

        if (expectedGate == 0)
        {
            if (car.CurrentGateIndex == numGates - 1)
                car.CompletedLaps++;
            car.CurrentGateIndex = 0;
        }
        else
        {
            car.CurrentGateIndex = expectedGate;
        }
    }

    private static bool SegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
    {
        var d1 = a2 - a1;
        var d2 = b2 - b1;
        float cross = d1.X * d2.Y - d1.Y * d2.X;
        if (MathF.Abs(cross) < 1e-10f) return false;
        var diff = b1 - a1;
        float t = (diff.X * d2.Y - diff.Y * d2.X) / cross;
        float u = (diff.X * d1.Y - diff.Y * d1.X) / cross;
        return t >= 0f && t <= 1f && u >= 0f && u <= 1f;
    }
}
