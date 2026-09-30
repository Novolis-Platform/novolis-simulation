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
