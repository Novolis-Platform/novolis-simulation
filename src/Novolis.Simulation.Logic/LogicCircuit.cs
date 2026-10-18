using System.Collections.ObjectModel;

namespace Novolis.Simulation.Logic;

/// <summary>
/// Immutable topology and node configuration for one logic circuit.
/// </summary>
public sealed class LogicCircuit
{
    /// <summary>The current serialized circuit schema version.</summary>
    public const int CurrentSchemaVersion = 1;

    private readonly ReadOnlyCollection<LogicNode> _nodes;
    private readonly ReadOnlyCollection<LogicConnection> _connections;

    /// <summary>Creates a circuit snapshot.</summary>
    public LogicCircuit(
        IEnumerable<LogicNode>? nodes = null,
        IEnumerable<LogicConnection>? connections = null)
    {
        var nodeList = (nodes ?? [])
            .Select(node => node ?? throw new ArgumentNullException(nameof(nodes)))
            .OrderBy(node => node.Id)
            .ToArray();
        var connectionList = (connections ?? [])
            .Select(connection =>
                connection ?? throw new ArgumentNullException(nameof(connections)))
            .OrderBy(connection => connection.Id)
            .ToArray();

        _nodes = new ReadOnlyCollection<LogicNode>(nodeList);
        _connections = new ReadOnlyCollection<LogicConnection>(connectionList);
    }

    /// <summary>Gets an empty circuit.</summary>
    public static LogicCircuit Empty { get; } = new();

    /// <summary>Gets nodes in stable identity order.</summary>
    public IReadOnlyList<LogicNode> Nodes => _nodes;

    /// <summary>Gets connections in stable identity order.</summary>
    public IReadOnlyList<LogicConnection> Connections => _connections;

    /// <summary>Looks up a node by identity.</summary>
    public bool TryGetNode(LogicNodeId id, out LogicNode? node)
    {
        node = _nodes.FirstOrDefault(candidate => candidate.Id == id);
        return node is not null;
    }

    /// <summary>Adds a node and returns a new snapshot.</summary>
    public LogicCircuit AddNode(LogicNode node)
    {
        ArgumentNullException.ThrowIfNull(node);
        if (_nodes.Any(candidate => candidate.Id == node.Id))
        {
            throw new ArgumentException(
                $"A node with identity '{node.Id}' already exists.",
                nameof(node));
        }

        return new LogicCircuit(_nodes.Append(node), _connections);
    }

    /// <summary>Removes a node and all attached connections.</summary>
    public LogicCircuit RemoveNode(LogicNodeId nodeId)
    {
        if (!_nodes.Any(node => node.Id == nodeId))
            return this;

        return new LogicCircuit(
            _nodes.Where(node => node.Id != nodeId),
            _connections.Where(connection =>
                connection.Source.NodeId != nodeId &&
                connection.Destination.NodeId != nodeId));
    }

    /// <summary>Moves one node in the neutral planar editor coordinate system.</summary>
    public LogicCircuit MoveNode(LogicNodeId nodeId, double x, double z)
    {
        if (!_nodes.Any(node => node.Id == nodeId))
            return this;

        return new LogicCircuit(
            _nodes.Select(node =>
                node.Id == nodeId ? node.WithPosition(x, z) : node),
            _connections);
    }

    /// <summary>Changes the configured value of an input node.</summary>
    public LogicCircuit SetInputValue(LogicNodeId nodeId, LogicValue value)
    {
        if (!TryGetNode(nodeId, out var node))
            return this;

        if (node!.Kind != LogicNodeKind.Input)
        {
            throw new InvalidOperationException(
                $"Node '{nodeId}' is not an input node.");
        }

        return new LogicCircuit(
            _nodes.Select(candidate =>
                candidate.Id == nodeId
                    ? candidate.WithInputValue(value)
                    : candidate),
            _connections);
    }

    /// <summary>
    /// Attempts to connect an output port to a single-input port.
    /// </summary>
    public LogicConnectionResult TryConnect(
        LogicConnectionId connectionId,
        LogicPortAddress source,
        LogicPortAddress destination,
        LogicChipLibrary? library = null)
    {
        var diagnostics = new List<LogicDiagnostic>();
        if (connectionId.Value == Guid.Empty)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.InvalidDefinition,
                LogicDiagnosticSeverity.Error,
                "A connection requires a non-empty identity.",
                connectionId: connectionId));
        }

        if (_connections.Any(connection => connection.Id == connectionId))
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.DuplicateConnectionId,
                LogicDiagnosticSeverity.Error,
                $"A connection with identity '{connectionId}' already exists.",
                connectionId: connectionId));
        }

        var sourceNode = FindNode(source.NodeId);
        var destinationNode = FindNode(destination.NodeId);
        var sourcePort = ResolvePort(
            sourceNode,
            source.PortId,
            library,
            LogicDiagnosticCode.MissingSourcePort,
            "source",
            connectionId,
            diagnostics);
        var destinationPort = ResolvePort(
            destinationNode,
            destination.PortId,
            library,
            LogicDiagnosticCode.MissingDestinationPort,
            "destination",
            connectionId,
            diagnostics);

        if (sourcePort is not null &&
            sourcePort.Direction != LogicPortDirection.Output)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.SourceIsNotOutput,
                LogicDiagnosticSeverity.Error,
                $"Port '{source.PortId}' on node '{source.NodeId}' is not an output.",
                source.NodeId,
                connectionId));
        }

        if (destinationPort is not null &&
            destinationPort.Direction != LogicPortDirection.Input)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.DestinationIsNotInput,
                LogicDiagnosticSeverity.Error,
                $"Port '{destination.PortId}' on node '{destination.NodeId}' is not an input.",
                destination.NodeId,
                connectionId));
        }

        if (_connections.Any(connection => connection.Destination == destination))
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.InputCardinalityExceeded,
                LogicDiagnosticSeverity.Error,
                $"Input '{destination.PortId}' on node '{destination.NodeId}' already has a connection.",
                destination.NodeId,
                connectionId));
        }

        diagnostics = SortDiagnostics(diagnostics).ToList();
        if (diagnostics.Count > 0)
        {
            return new LogicConnectionResult(false, this, diagnostics);
        }

        return new LogicConnectionResult(
            true,
            new LogicCircuit(
                _nodes,
                _connections.Append(new LogicConnection(
                    connectionId,
                    source,
                    destination))),
            []);
    }

    /// <summary>
    /// Attempts to connect two ports with a generated stable identity.
    /// </summary>
    public LogicConnectionResult TryConnect(
        LogicPortAddress source,
        LogicPortAddress destination,
        LogicChipLibrary? library = null) =>
        TryConnect(LogicConnectionId.New(), source, destination, library);

    /// <summary>Connects two ports or throws with the validation diagnostics.</summary>
    public LogicCircuit Connect(
        LogicConnectionId connectionId,
        LogicPortAddress source,
        LogicPortAddress destination,
        LogicChipLibrary? library = null)
    {
        var result = TryConnect(connectionId, source, destination, library);
        if (!result.IsValid)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    result.Diagnostics.Select(diagnostic => diagnostic.Message)));
        }

        return result.Circuit;
    }

    /// <summary>Removes a connection and returns a new snapshot.</summary>
    public LogicCircuit RemoveConnection(LogicConnectionId connectionId) =>
        new LogicCircuit(
            _nodes,
            _connections.Where(connection => connection.Id != connectionId));

    /// <summary>Validates topology, ports, cardinality, and custom references.</summary>
    public LogicValidationResult Validate(
        LogicChipLibrary? library = null,
        bool rejectCycles = true)
    {
        var diagnostics = new List<LogicDiagnostic>();

        foreach (var group in _nodes
                     .GroupBy(node => node.Id)
                     .Where(group => group.Count() > 1))
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.DuplicateNodeId,
                LogicDiagnosticSeverity.Error,
                $"Node identity '{group.Key}' occurs {group.Count()} times.",
                group.Key));
        }

        foreach (var group in _connections
                     .GroupBy(connection => connection.Id)
                     .Where(group => group.Count() > 1))
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.DuplicateConnectionId,
                LogicDiagnosticSeverity.Error,
                $"Connection identity '{group.Key}' occurs {group.Count()} times.",
                connectionId: group.Key));
        }

        foreach (var node in _nodes.Where(node => node.Kind == LogicNodeKind.CustomChip))
        {
            if (node.DefinitionId is null ||
                library is null ||
                !library.TryGet(node.DefinitionId, out _))
            {
                diagnostics.Add(new LogicDiagnostic(
                    LogicDiagnosticCode.MissingDefinition,
                    LogicDiagnosticSeverity.Error,
                    $"Custom node '{node.Id}' refers to missing definition '{node.DefinitionId ?? "<empty>"}'.",
                    node.Id));
            }
        }

        foreach (var connection in _connections)
            ValidateConnection(connection, library, diagnostics);

        var order = GetTopologicalOrder();
        if (rejectCycles && order.HasCycle)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.Cycle,
                LogicDiagnosticSeverity.Error,
                "The circuit contains a feedback cycle. The MVP evaluator requires a directed acyclic circuit."));
        }

        return new LogicValidationResult(SortDiagnostics(diagnostics));
    }

    /// <summary>
    /// Evaluates one deterministic tick of the circuit.
    /// </summary>
    public LogicEvaluationResult Evaluate(
        LogicChipLibrary? library = null,
        IReadOnlyDictionary<string, LogicValue>? externalInputs = null)
    {
        var validation = Validate(library);
        if (!validation.IsValid)
        {
            var diagnostics = validation.Diagnostics.ToList();
            if (diagnostics.Any(diagnostic =>
                    diagnostic.Code == LogicDiagnosticCode.Cycle))
            {
                diagnostics.Add(new LogicDiagnostic(
                    LogicDiagnosticCode.NonConvergent,
                    LogicDiagnosticSeverity.Error,
                    "Evaluation stopped because the circuit is not acyclic."));
            }

            return new LogicEvaluationResult(diagnostics: SortDiagnostics(diagnostics));
        }

        var order = GetTopologicalOrder().Order;
        var incoming = _connections.ToDictionary(
            connection => connection.Destination,
            connection => connection);
        var portValues = new Dictionary<LogicPortAddress, LogicValue>();
        var outputValues = new Dictionary<LogicNodeId, LogicValue>();
        var diagnosticsDuringEvaluation = new List<LogicDiagnostic>();

        foreach (var nodeId in order)
        {
            var node = FindNode(nodeId)!;
            var inputValues = new Dictionary<string, LogicValue>(
                StringComparer.Ordinal);
            foreach (var port in LogicNodeCatalog.GetPorts(node, library)
                         .Where(port => port.Direction == LogicPortDirection.Input))
            {
                var address = new LogicPortAddress(node.Id, port.Id);
                var value = ResolveIncomingValue(
                    address,
                    incoming,
                    portValues);
                portValues[address] = value;
                inputValues[port.Id] = value;
            }

            if (node.Kind == LogicNodeKind.Input)
            {
                var value = externalInputs is not null &&
                            externalInputs.TryGetValue(node.Label, out var externalValue)
                    ? externalValue
                    : node.InputValue;
                portValues[new LogicPortAddress(node.Id, "out")] = value;
                continue;
            }

            if (node.Kind == LogicNodeKind.Output)
            {
                var value = inputValues.TryGetValue("in", out var input)
                    ? input
                    : LogicValue.Disconnected;
                outputValues[node.Id] = value;
                continue;
            }

            if (node.Kind == LogicNodeKind.CustomChip)
            {
                EvaluateCustomNode(
                    node,
                    inputValues,
                    library,
                    portValues,
                    diagnosticsDuringEvaluation);
                continue;
            }

            var output = LogicTruthTables.Evaluate(
                node.Kind,
                LogicNodeCatalog.GetPorts(node, library)
                    .Where(port => port.Direction == LogicPortDirection.Input)
                    .Select(port => inputValues[port.Id])
                    .ToArray());
            portValues[new LogicPortAddress(node.Id, "out")] = output;
        }

        return new LogicEvaluationResult(
            portValues,
            outputValues,
            SortDiagnostics(diagnosticsDuringEvaluation));
    }

    private void EvaluateCustomNode(
        LogicNode node,
        IReadOnlyDictionary<string, LogicValue> inputValues,
        LogicChipLibrary? library,
        IDictionary<LogicPortAddress, LogicValue> portValues,
        ICollection<LogicDiagnostic> diagnostics)
    {
        if (library is null ||
            node.DefinitionId is null ||
            !library.TryGet(node.DefinitionId, out var definition) ||
            definition is null)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.MissingDefinition,
                LogicDiagnosticSeverity.Error,
                $"Custom node '{node.Id}' could not resolve definition '{node.DefinitionId ?? "<empty>"}'.",
                node.Id));
            return;
        }

        var childInputs = definition.InputNodes.Keys.ToDictionary(
            name => name,
            name => inputValues.TryGetValue(name, out var value)
                ? value
                : LogicValue.Disconnected,
            StringComparer.Ordinal);
        var childResult = definition.Circuit.Evaluate(library, childInputs);
        foreach (var diagnostic in childResult.Diagnostics)
            diagnostics.Add(diagnostic);

        foreach (var output in definition.OutputNodes)
        {
            var value = childResult.OutputValues.TryGetValue(
                output.Value,
                out var outputValue)
                ? outputValue
                : LogicValue.Disconnected;
            portValues[new LogicPortAddress(node.Id, output.Key)] = value;
        }
    }

    private void ValidateConnection(
        LogicConnection connection,
        LogicChipLibrary? library,
        ICollection<LogicDiagnostic> diagnostics)
    {
        var sourceNode = FindNode(connection.Source.NodeId);
        var destinationNode = FindNode(connection.Destination.NodeId);
        if (sourceNode is null)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.MissingSourceNode,
                LogicDiagnosticSeverity.Error,
                $"Connection '{connection.Id}' refers to missing source node '{connection.Source.NodeId}'.",
                connectionId: connection.Id));
        }

        if (destinationNode is null)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.MissingDestinationNode,
                LogicDiagnosticSeverity.Error,
                $"Connection '{connection.Id}' refers to missing destination node '{connection.Destination.NodeId}'.",
                connectionId: connection.Id));
        }

        if (sourceNode is null || destinationNode is null)
            return;

        LogicNodeCatalog.TryGetPort(
            sourceNode,
            connection.Source.PortId,
            library,
            out var sourcePort);
        LogicNodeCatalog.TryGetPort(
            destinationNode,
            connection.Destination.PortId,
            library,
            out var destinationPort);

        if (sourcePort is null)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.MissingSourcePort,
                LogicDiagnosticSeverity.Error,
                $"Node '{sourceNode.Id}' has no source port '{connection.Source.PortId}'.",
                sourceNode.Id,
                connection.Id));
        }

        if (destinationPort is null)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.MissingDestinationPort,
                LogicDiagnosticSeverity.Error,
                $"Node '{destinationNode.Id}' has no destination port '{connection.Destination.PortId}'.",
                destinationNode.Id,
                connection.Id));
        }

        if (sourcePort is not null &&
            sourcePort.Direction != LogicPortDirection.Output)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.SourceIsNotOutput,
                LogicDiagnosticSeverity.Error,
                $"Connection '{connection.Id}' starts at input port '{connection.Source.PortId}'.",
                sourceNode.Id,
                connection.Id));
        }

        if (destinationPort is not null &&
            destinationPort.Direction != LogicPortDirection.Input)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.DestinationIsNotInput,
                LogicDiagnosticSeverity.Error,
                $"Connection '{connection.Id}' ends at output port '{connection.Destination.PortId}'.",
                destinationNode.Id,
                connection.Id));
        }

        if (_connections.Count(candidate =>
                candidate.Destination == connection.Destination) > 1)
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.InputCardinalityExceeded,
                LogicDiagnosticSeverity.Error,
                $"Input '{connection.Destination.PortId}' on node '{connection.Destination.NodeId}' has more than one driver.",
                destinationNode.Id,
                connection.Id));
        }
    }

    private LogicPortDefinition? ResolvePort(
        LogicNode? node,
        string portId,
        LogicChipLibrary? library,
        LogicDiagnosticCode missingCode,
        string endpointName,
        LogicConnectionId connectionId,
        ICollection<LogicDiagnostic> diagnostics)
    {
        if (node is null)
        {
            diagnostics.Add(new LogicDiagnostic(
                endpointName == "source"
                    ? LogicDiagnosticCode.MissingSourceNode
                    : LogicDiagnosticCode.MissingDestinationNode,
                LogicDiagnosticSeverity.Error,
                $"Connection '{connectionId}' refers to a missing {endpointName} node.",
                connectionId: connectionId));
            return null;
        }

        if (node.Kind == LogicNodeKind.CustomChip &&
            (node.DefinitionId is null ||
             library is null ||
             !library.TryGet(node.DefinitionId, out _)))
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.MissingDefinition,
                LogicDiagnosticSeverity.Error,
                $"Custom node '{node.Id}' has no resolvable chip definition.",
                node.Id,
                connectionId));
            return null;
        }

        if (!LogicNodeCatalog.TryGetPort(node, portId, library, out var port))
        {
            diagnostics.Add(new LogicDiagnostic(
                missingCode,
                LogicDiagnosticSeverity.Error,
                $"Node '{node.Id}' has no {endpointName} port '{portId}'.",
                node.Id,
                connectionId));
            return null;
        }

        return port;
    }

    private LogicNode? FindNode(LogicNodeId id) =>
        _nodes.FirstOrDefault(node => node.Id == id);

    private TopologicalOrder GetTopologicalOrder()
    {
        var nodeIds = _nodes
            .Select(node => node.Id)
            .Distinct()
            .Order()
            .ToArray();
        var adjacency = nodeIds.ToDictionary(
            id => id,
            _ => new SortedSet<LogicNodeId>());
        var indegree = nodeIds.ToDictionary(id => id, _ => 0);

        foreach (var connection in _connections)
        {
            if (!adjacency.ContainsKey(connection.Source.NodeId) ||
                !adjacency.ContainsKey(connection.Destination.NodeId))
            {
                continue;
            }

            if (adjacency[connection.Source.NodeId]
                .Add(connection.Destination.NodeId))
            {
                indegree[connection.Destination.NodeId]++;
            }
        }

        var ready = new SortedSet<LogicNodeId>(
            indegree
                .Where(pair => pair.Value == 0)
                .Select(pair => pair.Key));
        var order = new List<LogicNodeId>(nodeIds.Length);
        while (ready.Count > 0)
        {
            var current = ready.Min;
            ready.Remove(current);
            order.Add(current);
            foreach (var next in adjacency[current])
            {
                indegree[next]--;
                if (indegree[next] == 0)
                    ready.Add(next);
            }
        }

        return new TopologicalOrder(
            order,
            order.Count != nodeIds.Length);
    }

    private static IReadOnlyList<LogicDiagnostic> SortDiagnostics(
        IEnumerable<LogicDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(diagnostic => diagnostic.Code)
            .ThenBy(diagnostic => diagnostic.NodeId)
            .ThenBy(diagnostic => diagnostic.ConnectionId)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToArray();

    private static LogicValue ResolveIncomingValue(
        LogicPortAddress destination,
        IReadOnlyDictionary<LogicPortAddress, LogicConnection> incoming,
        IReadOnlyDictionary<LogicPortAddress, LogicValue> portValues) =>
        incoming.TryGetValue(destination, out var connection) &&
        portValues.TryGetValue(connection.Source, out var value)
            ? value
            : LogicValue.Disconnected;

    private readonly record struct TopologicalOrder(
        IReadOnlyList<LogicNodeId> Order,
        bool HasCycle);
}
