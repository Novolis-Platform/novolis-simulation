namespace Novolis.Simulation.Racing.Race;

using Novolis.Simulation.Racing.Cars;

/// <summary>Represents RaceWorldState.</summary>
public sealed class RaceWorldState
{
    /// <summary>Tick.</summary>
    public required int Tick { get; set; }
    /// <summary>Cars.</summary>
    public required IReadOnlyList<CarState> Cars { get; init; }
    /// <summary>Leaderboard.</summary>
    public required RaceLeaderboard Leaderboard { get; set; }
    /// <summary>IsFinished.</summary>
    public required bool IsFinished { get; set; }
}
