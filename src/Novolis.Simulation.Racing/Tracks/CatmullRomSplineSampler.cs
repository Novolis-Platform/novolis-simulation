namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents CatmullRomSplineSampler.</summary>
public sealed class CatmullRomSplineSampler : ISplineSampler
{
    /// <summary>Samples a value at the given coordinates.</summary>
    public SplineSample Sample(SplineLoop spline, double t)
    {
        var points = spline.ControlPoints;
        int n = points.Count;

        int segmentIndex = (int)(t * n) % n;
        double u = (t * n) - (int)(t * n);

        var p0 = points[((segmentIndex - 1) + n) % n];
        var p1 = points[segmentIndex];
        var p2 = points[(segmentIndex + 1) % n];
        var p3 = points[(segmentIndex + 2) % n];

        var pos = 0.5f * ((2 * p1)
            + (-p0 + p2) * (float)u
            + (2 * p0 - 5 * p1 + 4 * p2 - p3) * (float)(u * u)
            + (-p0 + 3 * p1 - 3 * p2 + p3) * (float)(u * u * u));

        var tanUnnorm = 0.5f * ((-p0 + p2)
            + 2 * (2 * p0 - 5 * p1 + 4 * p2 - p3) * (float)u
            + 3 * (-p0 + 3 * p1 - 3 * p2 + p3) * (float)(u * u));

        Vector2 tangent;
        if (tanUnnorm.LengthSquared() < 1e-10f)
            tangent = Vector2.UnitX;
        else
            tangent = Vector2.Normalize(tanUnnorm);

        var normal = new Vector2(tangent.Y, -tangent.X);

        return new SplineSample(pos, tangent, normal);
    }

    /// <summary>Samples a value at the given coordinates.</summary>
    public IReadOnlyList<SplineSample> SampleEvenly(SplineLoop spline, int count) =>
        Enumerable.Range(0, count).Select(i => Sample(spline, i / (double)count)).ToArray();
}
