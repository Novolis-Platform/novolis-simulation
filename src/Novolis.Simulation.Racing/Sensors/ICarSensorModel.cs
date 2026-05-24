namespace Novolis.Simulation.Racing.Sensors;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Tracks;

public interface ICarSensorModel
{
    IReadOnlyList<SensorDefinition> Definitions { get; }
    SensorReading Read(RaceTrack track, CarState car);
}
