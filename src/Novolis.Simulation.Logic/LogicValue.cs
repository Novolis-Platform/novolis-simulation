namespace Novolis.Simulation.Logic;

/// <summary>
/// A value carried by one logic port.
/// </summary>
public readonly record struct LogicValue(LogicSignal Signal)
{
    /// <summary>Gets a low signal.</summary>
    public static LogicValue Low { get; } = new(LogicSignal.Low);

    /// <summary>Gets a high signal.</summary>
    public static LogicValue High { get; } = new(LogicSignal.High);

    /// <summary>Gets a disconnected signal.</summary>
    public static LogicValue Disconnected { get; } = new(LogicSignal.Disconnected);

    /// <summary>Gets an unresolved signal.</summary>
    public static LogicValue Unknown { get; } = new(LogicSignal.Unknown);

    /// <summary>Gets a conflicting signal.</summary>
    public static LogicValue Contention { get; } = new(LogicSignal.Contention);

    /// <summary>Gets whether the signal is a resolved Boolean.</summary>
    public bool IsKnown =>
        Signal is LogicSignal.Low or LogicSignal.High;

    /// <summary>Gets whether the signal is disconnected.</summary>
    public bool IsDisconnected => Signal == LogicSignal.Disconnected;

    /// <summary>Creates a signal from a Boolean value.</summary>
    public static LogicValue FromBoolean(bool value) =>
        value ? High : Low;

    /// <summary>Returns the signal as a compact display value.</summary>
    public override string ToString() =>
        Signal switch
        {
            LogicSignal.Low => "0",
            LogicSignal.High => "1",
            LogicSignal.Disconnected => "Z",
            LogicSignal.Unknown => "?",
            LogicSignal.Contention => "X",
            _ => Signal.ToString(),
        };
}
