namespace Novolis.Simulation.Racing.Cars;

public sealed record RaceStanding(int Position, int CarId, string Name, int CompletedLaps, double TrackProgress);
