namespace Novolis.Simulation.View;

/// <summary>Produces a <see cref="ViewPose"/> each tick from input and time.</summary>
public interface IViewController
{
    ViewPose Pose { get; }

    void Tick(float deltaSeconds);
}
