namespace Novolis.Simulation.Logic;

/// <summary>
/// Describes one named port on a logic node.
/// </summary>
public sealed record LogicPortDefinition
{
    /// <summary>Creates a port definition.</summary>
    public LogicPortDefinition(
        string id,
        LogicPortDirection direction,
        bool required = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id = id.Trim();
        Direction = direction;
        Required = required;
    }

    /// <summary>Gets the stable port identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the port direction.</summary>
    public LogicPortDirection Direction { get; }

    /// <summary>Gets whether the port must have a connection.</summary>
    public bool Required { get; }
}
