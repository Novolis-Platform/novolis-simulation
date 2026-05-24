namespace Novolis.Simulation.View;

/// <summary>Produces a <see cref="ViewPose"/> each tick from input and time.</summary>
public interface IViewController
{
    /// <summary>Pose.</summary>
    ViewPose Pose { get; }

    /// <summary>Tick operation.</summary>
    void Tick(float deltaSeconds);
}
