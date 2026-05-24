namespace Novolis.Simulation.Racing.Progress;

public readonly record struct TrackProgressSample(
    double LoopT,
    double TotalProgress,
    double Alignment,
    double SignedCenterOffset,
    bool IsWrongWay);
