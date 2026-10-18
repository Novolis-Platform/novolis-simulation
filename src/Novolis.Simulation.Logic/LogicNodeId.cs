namespace Novolis.Simulation.Logic;

/// <summary>
/// Stable identity for a circuit node.
/// </summary>
public readonly record struct LogicNodeId(Guid Value) : IComparable<LogicNodeId>
{
    /// <summary>Creates a new node identity.</summary>
    public static LogicNodeId New() => new(Guid.NewGuid());

    /// <summary>Parses a serialized node identity.</summary>
    public static LogicNodeId Parse(string value) =>
        new(Guid.Parse(value));

    /// <inheritdoc />
    public int CompareTo(LogicNodeId other) => Value.CompareTo(other.Value);

    /// <inheritdoc />
    public override string ToString() => Value.ToString("N");
}
