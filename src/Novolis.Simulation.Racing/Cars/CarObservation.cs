namespace Novolis.Simulation.Racing.Cars;

using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Sensors;

/// <summary>Per-tick observation passed to <see cref="IRaceCarController"/> implementations.</summary>
/// <param name="CarIndex">Zero-based index of the car in the race grid.</param>
/// <param name="State">Mutable kinematic and scoring state for the car.</param>
/// <param name="Sensors">Ray-cast sensor readings relative to the car pose.</param>
/// <param name="Progress">Resolved progress along the track centerline.</param>
/// <param name="Standing">Current leaderboard standing for the car.</param>
public readonly record struct CarObservation(
    int CarIndex,
    CarState State,
    SensorReading Sensors,
    TrackProgressSample Progress,
    RaceStanding Standing);
