namespace Novolis.Simulation.Racing.Rewards;

/// <summary>Decomposed reward terms used by <see cref="IRewardModel"/> implementations.</summary>
/// <param name="ForwardProgressReward">Reward for advancing along the track.</param>
/// <param name="SpeedReward">Reward for maintaining target speed.</param>
/// <param name="WallProximityPenalty">Penalty for driving near track walls.</param>
/// <param name="WrongWayPenalty">Penalty for driving against race direction.</param>
/// <param name="CrashPenalty">Penalty applied when the car is crashed.</param>
/// <param name="GateReward">Bonus for crossing intermediate track gates.</param>
/// <param name="LapReward">Bonus for completing a full lap.</param>
public sealed record RewardBreakdown(
    double ForwardProgressReward,
    double SpeedReward,
    double WallProximityPenalty,
    double WrongWayPenalty,
    double CrashPenalty,
    double GateReward,
    double LapReward)
{
    /// <summary>Net reward after summing bonuses and penalties.</summary>
    public double Total =>
        ForwardProgressReward + SpeedReward
        - WallProximityPenalty - WrongWayPenalty - CrashPenalty
        + GateReward + LapReward;
}
