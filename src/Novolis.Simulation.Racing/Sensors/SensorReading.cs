namespace Novolis.Simulation.Racing.Sensors;

public sealed record SensorReading(double[] Values, SensorRayHit[] Hits);
