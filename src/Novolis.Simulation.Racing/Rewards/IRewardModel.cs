namespace Novolis.Simulation.Racing.Rewards;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Tracks;

public interface IRewardModel
{
    RewardBreakdown Evaluate(
        RaceTrack track,
        CarState previous,
        CarState current,
        TrackProgressSample previousProgress,
        TrackProgressSample currentProgress);
}
