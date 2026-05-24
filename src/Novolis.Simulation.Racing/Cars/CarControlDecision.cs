namespace Novolis.Simulation.Racing.Cars;

/// <summary>Brake.</summary>
/// <summary>Throttle.</summary>
/// <summary>Steering.</summary>
/// <summary>CarControlDecision operation.</summary>
/// <summary>Represents CarControlDecision.</summary>
public readonly record struct CarControlDecision(double Steering, double Throttle, double Brake);
