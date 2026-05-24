namespace Novolis.Simulation.Racing.Rewards;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Tracks;

/// <summary>Represents IRewardModel.</summary>
public interface IRewardModel
{
    /// <summary>Evaluate operation.</summary>
    RewardBreakdown Evaluate(
        RaceTrack track,
        CarState previous,
        CarState current,
        TrackProgressSample previousProgress,
        TrackProgressSample currentProgress);
}
