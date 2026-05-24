namespace Novolis.Simulation.Racing.Rewards;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Tracks;

public sealed class DefaultRewardModel : IRewardModel
{
    private const double ForwardProgressScale = 10.0;
    private const double SpeedScale = 0.1;
    private const double WrongWayPenaltyValue = 0.5;
    private const double CrashPenaltyValue = 5.0;
    private const double GateRewardValue = 2.0;
    private const double LapRewardValue = 10.0;

    public RewardBreakdown Evaluate(
        RaceTrack track,
        CarState previous,
        CarState current,
        TrackProgressSample previousProgress,
        TrackProgressSample currentProgress)
    {
        double raw = currentProgress.LoopT - previousProgress.LoopT;
        if (raw < -0.5) raw += 1.0;
        double forwardProgressReward = Math.Max(0, raw) * ForwardProgressScale;

        double speedReward = current.Speed > 0.5 ? current.Speed * SpeedScale : 0;

        double wrongWayPenalty = currentProgress.IsWrongWay ? WrongWayPenaltyValue : 0;
        double crashPenalty = current.Crashed ? CrashPenaltyValue : 0;

        int numGates = track.Gates.Count;
        bool gateAdvanced = false;
        if (numGates > 0)
        {
            int prevGate = previous.CurrentGateIndex;
            int currGate = current.CurrentGateIndex;
            if (currGate != prevGate)
                gateAdvanced = (currGate == (prevGate + 1) % numGates);
        }

        double gateReward = gateAdvanced ? GateRewardValue : 0;
        double lapReward = current.CompletedLaps > previous.CompletedLaps ? LapRewardValue : 0;

        return new RewardBreakdown(
            forwardProgressReward,
            speedReward,
            0,
            wrongWayPenalty,
            crashPenalty,
            gateReward,
            lapReward);
    }
}
