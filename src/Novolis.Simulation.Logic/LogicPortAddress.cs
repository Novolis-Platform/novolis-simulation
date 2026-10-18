namespace Novolis.Simulation.Logic;

/// <summary>
/// Identifies a port on a circuit node.
/// </summary>
public readonly record struct LogicPortAddress
{
    /// <summary>Gets the node identity.</summary>
    public LogicNodeId NodeId { get; }

    /// <summary>Gets the port identifier.</summary>
    public string PortId { get; }

    /// <summary>
    /// Creates a port address after validating its port identifier.
    /// </summary>
    public LogicPortAddress(LogicNodeId nodeId, string portId) : this()
    {
        if (nodeId.Value == Guid.Empty)
            throw new ArgumentException("A port address requires a node identity.", nameof(nodeId));

        ArgumentException.ThrowIfNullOrWhiteSpace(portId);
        NodeId = nodeId;
        PortId = portId.Trim();
    }
}
