namespace Novolis.Simulation.Racing.Rewards;

public sealed record RewardBreakdown(
    double ForwardProgressReward,
    double SpeedReward,
    double WallProximityPenalty,
    double WrongWayPenalty,
    double CrashPenalty,
    double GateReward,
    double LapReward)
{
    public double Total =>
        ForwardProgressReward + SpeedReward
        - WallProximityPenalty - WrongWayPenalty - CrashPenalty
        + GateReward + LapReward;
}
