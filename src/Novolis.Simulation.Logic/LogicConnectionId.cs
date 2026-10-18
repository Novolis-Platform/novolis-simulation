namespace Novolis.Simulation.Logic;

/// <summary>
/// Stable identity for a circuit connection.
/// </summary>
public readonly record struct LogicConnectionId(Guid Value) : IComparable<LogicConnectionId>
{
    /// <summary>Creates a new connection identity.</summary>
    public static LogicConnectionId New() => new(Guid.NewGuid());

    /// <summary>Parses a serialized connection identity.</summary>
    public static LogicConnectionId Parse(string value) =>
        new(Guid.Parse(value));

    /// <inheritdoc />
    public int CompareTo(LogicConnectionId other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
