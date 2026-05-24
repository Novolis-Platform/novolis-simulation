namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public static class TrackSpecs
{
    /// <summary>Evenly spaced gate fractions in [0, 1) along the spline loop.</summary>
    public static IReadOnlyList<double> UniformGateSamples(int gateCount) =>
        gateCount <= 0 ? [] : Enumerable.Range(0, gateCount).Select(i => i / (double)gateCount).ToArray();

    /// <summary>Closed polyline circuit with uniform gates and start at sample 0.</summary>
    public static TrackBuildSpec Polyline(
        int rasterWidth,
        int rasterHeight,
        double trackHalfWidth,
        double wallThickness,
        int lapsToFinish,
        IReadOnlyList<Vector2> controlPoints,
        int gateCount,
        double startSample = 0) =>
        new(
            rasterWidth,
            rasterHeight,
            trackHalfWidth,
            wallThickness,
            lapsToFinish,
            new SplineLoop(controlPoints),
            UniformGateSamples(gateCount),
            startSample);

    public static TrackBuildSpec Circle(
        string name, int rasterWidth, int rasterHeight,
        Vector2 center, float radiusX, float radiusY,
        double trackHalfWidth, int controlPoints = 16, int gates = 12, int lapsToFinish = 5)
    {
        _ = name;
        var points = Enumerable.Range(0, controlPoints)
            .Select(i =>
            {
                var angle = MathF.Tau * i / controlPoints;
                return center + new Vector2(MathF.Cos(angle) * radiusX, MathF.Sin(angle) * radiusY);
            }).ToArray();
        return new TrackBuildSpec(
            rasterWidth, rasterHeight, trackHalfWidth, 1.0, lapsToFinish,
            new SplineLoop(points),
            Enumerable.Range(0, gates).Select(i => i / (double)gates).ToArray(),
            0.0);
    }
}
