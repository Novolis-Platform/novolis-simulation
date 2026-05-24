namespace Novolis.Simulation.Racing.Race;

using Novolis.Simulation.Racing.Cars;

/// <summary>Represents RaceLeaderboard.</summary>
public sealed class RaceLeaderboard
{
    /// <summary>Standings.</summary>
    public required IReadOnlyList<RaceStanding> Standings { get; init; }
}
