namespace Novolis.Simulation.Logic;

/// <summary>
/// The resolved state of a single digital signal.
/// </summary>
public enum LogicSignal
{
    /// <summary>Resolved low signal.</summary>
    Low,
    /// <summary>Resolved high signal.</summary>
    High,
    /// <summary>No driver is connected.</summary>
    Disconnected,
    /// <summary>The signal could not be resolved.</summary>
    Unknown,
    /// <summary>Drivers disagree about the signal.</summary>
    Contention,
}
