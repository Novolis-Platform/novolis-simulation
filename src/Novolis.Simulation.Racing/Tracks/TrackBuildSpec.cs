namespace Novolis.Simulation.Racing.Tracks;

/// <summary>
/// Inputs required to rasterize a spline track into a <see cref="RaceTrack"/> grid:
/// raster size, road half-width, wall thickness, lap count, centerline spline, gate samples, and start sample.
/// </summary>
public sealed record TrackBuildSpec(
    int RasterWidth,
    int RasterHeight,
    double TrackHalfWidth,
    double WallThickness,
    int LapsToFinish,
    SplineLoop CenterLine,
    IReadOnlyList<double> GateSamples,
    double StartSample);
