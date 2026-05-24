namespace Novolis.Simulation.Racing.Cars;

using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Sensors;

public readonly record struct CarObservation(
    int CarIndex,
    CarState State,
    SensorReading Sensors,
    TrackProgressSample Progress,
    RaceStanding Standing);
