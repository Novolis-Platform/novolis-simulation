namespace Novolis.Simulation.Racing.Race;

using Novolis.Simulation.Racing.Cars;

public sealed class RaceLeaderboard
{
    public required IReadOnlyList<RaceStanding> Standings { get; init; }
}
