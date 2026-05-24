namespace Novolis.Simulation.Racing.Tracks;

public interface ISplineSampler
{
    SplineSample Sample(SplineLoop spline, double t);
    IReadOnlyList<SplineSample> SampleEvenly(SplineLoop spline, int count);
}
