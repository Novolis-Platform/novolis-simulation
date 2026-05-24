namespace Novolis.Simulation.Racing.Tracks;

/// <summary>Represents ISplineSampler.</summary>
public interface ISplineSampler
{
    /// <summary>Samples a value at the given coordinates.</summary>
    SplineSample Sample(SplineLoop spline, double t);
    /// <summary>Samples a value at the given coordinates.</summary>
    IReadOnlyList<SplineSample> SampleEvenly(SplineLoop spline, int count);
}
