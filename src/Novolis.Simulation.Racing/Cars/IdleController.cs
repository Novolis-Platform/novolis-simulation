namespace Novolis.Simulation.Racing.Cars;

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
