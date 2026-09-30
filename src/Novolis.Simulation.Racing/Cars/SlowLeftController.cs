namespace Novolis.Simulation.Racing.Cars;

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
