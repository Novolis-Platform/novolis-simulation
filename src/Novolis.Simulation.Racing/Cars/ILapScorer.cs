namespace Novolis.Simulation.Racing.Cars;

using Novolis.Simulation.Racing.Tracks;

/// <summary>Represents ILapScorer.</summary>
public interface ILapScorer
{
    /// <summary>Update operation.</summary>
    void Update(RaceTrack track, CarState car);
}
