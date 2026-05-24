namespace Novolis.Simulation.Racing.Sensors;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Tracks;

/// <summary>Represents ICarSensorModel.</summary>
public interface ICarSensorModel
{
    /// <summary>Definitions.</summary>
    IReadOnlyList<SensorDefinition> Definitions { get; }
    /// <summary>Read operation.</summary>
    SensorReading Read(RaceTrack track, CarState car);
}
