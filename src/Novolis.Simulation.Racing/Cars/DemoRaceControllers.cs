namespace Novolis.Simulation.Racing.Cars;

/// <summary>Built-in demo controllers for simulations and tooling (no UI).</summary>
public sealed class FullThrottleController : IRaceCarController
{
    /// <summary>Name.</summary>
    public string Name => "FullThrottle";
    /// <summary>VisualStyle.</summary>
    public CarVisualStyle VisualStyle => new("A", "#FF4444");
    /// <summary>Decide operation.</summary>
    public CarControlDecision Decide(in CarObservation obs) => new(0, 1.0, 0);
}

/// <summary>Represents SlowLeftController.</summary>
public sealed class SlowLeftController : IRaceCarController
{
    /// <summary>Name.</summary>
    public string Name => "SlowLeft";
    /// <summary>VisualStyle.</summary>
    public CarVisualStyle VisualStyle => new("B", "#44AAFF");
    /// <summary>Decide operation.</summary>
    public CarControlDecision Decide(in CarObservation obs) => new(-0.35, 0.35, 0);
}

/// <summary>Represents IdleController.</summary>
public sealed class IdleController : IRaceCarController
{
    /// <summary>Name.</summary>
    public string Name => "Idle";
    /// <summary>VisualStyle.</summary>
    public CarVisualStyle VisualStyle => new("C", "#888888");
    /// <summary>Decide operation.</summary>
    public CarControlDecision Decide(in CarObservation obs) => new(0, 0, 0);
}
