namespace Novolis.Simulation.Racing.Tracks;

public sealed record TrackBuildSpec(
    int RasterWidth,
    int RasterHeight,
    double TrackHalfWidth,
    double WallThickness,
    int LapsToFinish,
    SplineLoop CenterLine,
    IReadOnlyList<double> GateSamples,
    double StartSample);
