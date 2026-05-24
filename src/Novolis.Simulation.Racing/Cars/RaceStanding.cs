namespace Novolis.Simulation.Racing.Cars;

/// <summary>TrackProgress.</summary>
/// <summary>CompletedLaps.</summary>
/// <summary>Name.</summary>
/// <summary>CarId.</summary>
/// <summary>Position.</summary>
/// <summary>RaceStanding operation.</summary>
/// <summary>Represents RaceStanding.</summary>
public sealed record RaceStanding(int Position, int CarId, string Name, int CompletedLaps, double TrackProgress);
