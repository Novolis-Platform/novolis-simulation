namespace Novolis.Simulation.Logic;

/// <summary>
/// Provides the port contracts for built-in and saved custom nodes.
/// </summary>
public static class LogicNodeCatalog
{
    private static readonly LogicPortDefinition InputOutput =
        new("out", LogicPortDirection.Output);

    private static readonly LogicPortDefinition OutputInput =
        new("in", LogicPortDirection.Input);

    private static readonly LogicPortDefinition GateInputA =
        new("a", LogicPortDirection.Input);

    private static readonly LogicPortDefinition GateInputB =
        new("b", LogicPortDirection.Input);

    private static readonly LogicPortDefinition GateInput =
        new("in", LogicPortDirection.Input);

    private static readonly LogicPortDefinition GateOutput =
        new("out", LogicPortDirection.Output);

    /// <summary>Gets the ports supported by a node.</summary>
    public static IReadOnlyList<LogicPortDefinition> GetPorts(
        LogicNode node,
        LogicChipLibrary? library = null)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (node.Kind == LogicNodeKind.CustomChip &&
            node.DefinitionId is not null &&
            library is not null &&
            library.TryGet(node.DefinitionId, out var definition) &&
            definition is not null)
        {
            return definition.Ports;
        }

        return node.Kind switch
        {
            LogicNodeKind.Input => [InputOutput],
            LogicNodeKind.Output => [OutputInput],
            LogicNodeKind.Not => [GateInput, GateOutput],
            LogicNodeKind.And or
                LogicNodeKind.Or or
                LogicNodeKind.Xor or
                LogicNodeKind.Nand => [GateInputA, GateInputB, GateOutput],
            _ => [],
        };
    }

    /// <summary>Looks up a port by its stable identifier.</summary>
    public static bool TryGetPort(
        LogicNode node,
        string portId,
        LogicChipLibrary? library,
        out LogicPortDefinition? port)
    {
        ArgumentNullException.ThrowIfNull(node);
        ArgumentException.ThrowIfNullOrWhiteSpace(portId);
        port = GetPorts(node, library)
            .FirstOrDefault(candidate =>
                string.Equals(candidate.Id, portId, StringComparison.Ordinal));
        return port is not null;
    }
}
