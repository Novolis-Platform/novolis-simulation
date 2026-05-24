namespace Novolis.Simulation.Racing.Progress;

using System.Numerics;

using Novolis.Simulation.Racing.Tracks;

public interface ITrackProgressResolver
{
    TrackProgressSample Resolve(RaceTrack track, Vector2 position, Vector2 forward);
}
