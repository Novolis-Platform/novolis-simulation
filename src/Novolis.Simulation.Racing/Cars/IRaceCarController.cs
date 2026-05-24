namespace Novolis.Simulation.Racing.Cars;

public interface IRaceCarController
{
    string Name { get; }
    CarVisualStyle VisualStyle { get; }
    CarControlDecision Decide(in CarObservation observation);
}
