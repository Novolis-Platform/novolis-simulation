namespace Novolis.Simulation.Racing.Cars;

/// <summary>Represents IRaceCarController.</summary>
public interface IRaceCarController
{
    /// <summary>Name.</summary>
    string Name { get; }
    /// <summary>VisualStyle.</summary>
    CarVisualStyle VisualStyle { get; }
    /// <summary>Decide operation.</summary>
    CarControlDecision Decide(in CarObservation observation);
}
