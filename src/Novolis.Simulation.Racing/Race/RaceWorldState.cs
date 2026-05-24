namespace Novolis.Simulation.Racing.Race;

using Novolis.Simulation.Racing.Cars;

public sealed class RaceWorldState
{
    public required int Tick { get; set; }
    public required IReadOnlyList<CarState> Cars { get; init; }
    public required RaceLeaderboard Leaderboard { get; set; }
    public required bool IsFinished { get; set; }
}
