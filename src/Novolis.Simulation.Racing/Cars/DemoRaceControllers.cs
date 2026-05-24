namespace Novolis.Simulation.Racing.Cars;

/// <summary>Built-in demo controllers for simulations and tooling (no UI).</summary>
public sealed class FullThrottleController : IRaceCarController
{
    public string Name => "FullThrottle";
    public CarVisualStyle VisualStyle => new("A", "#FF4444");
    public CarControlDecision Decide(in CarObservation obs) => new(0, 1.0, 0);
}

public sealed class SlowLeftController : IRaceCarController
{
    public string Name => "SlowLeft";
    public CarVisualStyle VisualStyle => new("B", "#44AAFF");
    public CarControlDecision Decide(in CarObservation obs) => new(-0.35, 0.35, 0);
}

public sealed class IdleController : IRaceCarController
{
    public string Name => "Idle";
    public CarVisualStyle VisualStyle => new("C", "#888888");
    public CarControlDecision Decide(in CarObservation obs) => new(0, 0, 0);
}
