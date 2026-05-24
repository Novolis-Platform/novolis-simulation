namespace Novolis.Simulation.Racing.Progress;

using System.Numerics;

using Novolis.Simulation.Racing.Tracks;

/// <summary>Represents ITrackProgressResolver.</summary>
public interface ITrackProgressResolver
{
    /// <summary>Resolve operation.</summary>
    TrackProgressSample Resolve(RaceTrack track, Vector2 position, Vector2 forward);
}
