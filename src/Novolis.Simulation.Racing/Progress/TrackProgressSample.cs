namespace Novolis.Simulation.Racing.Progress;

/// <summary>Progress of a car along a closed track spline.</summary>
/// <param name="LoopT">Normalized position on the current lap in [0, 1).</param>
/// <param name="TotalProgress">Lap count plus fractional progress along the current lap.</param>
/// <param name="Alignment">Dot product between car forward and track tangent (1 = aligned).</param>
/// <param name="SignedCenterOffset">Signed lateral offset from the track centerline.</param>
/// <param name="IsWrongWay">Whether the car is facing against the nominal race direction.</param>
public readonly record struct TrackProgressSample(
    double LoopT,
    double TotalProgress,
    double Alignment,
    double SignedCenterOffset,
    bool IsWrongWay);
