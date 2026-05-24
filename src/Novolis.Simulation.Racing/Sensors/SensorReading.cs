namespace Novolis.Simulation.Racing.Sensors;

/// <summary>Hits.</summary>
/// <summary>Values.</summary>
/// <summary>SensorReading operation.</summary>
/// <summary>Represents SensorReading.</summary>
public sealed record SensorReading(double[] Values, SensorRayHit[] Hits);
