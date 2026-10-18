using System.Text.Json;
using System.Text.Json.Serialization;

namespace Novolis.Simulation.Logic;

/// <summary>
/// Versioned JSON persistence for circuit structure and configured inputs.
/// </summary>
public static class LogicCircuitJson
{
    private static readonly JsonSerializerOptions Options = CreateOptions();

    /// <summary>Serializes a circuit using the current schema.</summary>
    public static string Serialize(LogicCircuit circuit, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(circuit);
        var document = new CircuitDocument
        {
            SchemaVersion = LogicCircuit.CurrentSchemaVersion,
            Nodes = circuit.Nodes.Select(ToDocument).ToList(),
            Connections = circuit.Connections.Select(ToDocument).ToList(),
        };

        return JsonSerializer.Serialize(
            document,
            indented ? Options : new JsonSerializerOptions(Options)
            {
                WriteIndented = false,
            });
    }

    /// <summary>Deserializes a versioned circuit document.</summary>
    public static LogicCircuit Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var document = JsonSerializer.Deserialize<CircuitDocument>(json, Options)
                       ?? throw new JsonException("The circuit document was empty.");
        if (document.SchemaVersion != LogicCircuit.CurrentSchemaVersion)
        {
            throw new JsonException(
                $"Unsupported circuit schema version {document.SchemaVersion}.");
        }

        try
        {
            var nodes = (document.Nodes ?? [])
                .Select(node => new LogicNode(
                    LogicNodeId.Parse(node.Id),
                    node.Kind,
                    node.Label,
                    node.X,
                    node.Z,
                    node.DefinitionId,
                    new LogicValue(node.InputSignal)))
                .ToArray();
            var connections = (document.Connections ?? [])
                .Select(connection => new LogicConnection(
                    LogicConnectionId.Parse(connection.Id),
                    new LogicPortAddress(
                        LogicNodeId.Parse(connection.SourceNodeId),
                        connection.SourcePort),
                    new LogicPortAddress(
                        LogicNodeId.Parse(connection.DestinationNodeId),
                        connection.DestinationPort)))
                .ToArray();
            return new LogicCircuit(nodes, connections);
        }
        catch (FormatException exception)
        {
            throw new JsonException(
                "The circuit document contains an invalid identity.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new JsonException(
                "The circuit document contains an invalid value.",
                exception);
        }
    }

    /// <summary>Attempts to deserialize a circuit without throwing.</summary>
    public static bool TryDeserialize(
        string json,
        out LogicCircuit? circuit,
        out string? error)
    {
        try
        {
            circuit = Deserialize(json);
            error = null;
            return true;
        }
        catch (JsonException exception)
        {
            circuit = null;
            error = exception.Message;
            return false;
        }
    }

    internal static JsonSerializerOptions SerializerOptions => Options;

    private static NodeDocument ToDocument(LogicNode node) =>
        new()
        {
            Id = node.Id.ToString(),
            Kind = node.Kind,
            Label = node.Label,
            X = node.X,
            Z = node.Z,
            DefinitionId = node.DefinitionId,
            InputSignal = node.InputValue.Signal,
        };

    private static ConnectionDocument ToDocument(LogicConnection connection) =>
        new()
        {
            Id = connection.Id.ToString(),
            SourceNodeId = connection.Source.NodeId.ToString(),
            SourcePort = connection.Source.PortId,
            DestinationNodeId = connection.Destination.NodeId.ToString(),
            DestinationPort = connection.Destination.PortId,
        };

    private static JsonSerializerOptions CreateOptions() =>
        new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

    internal sealed class CircuitDocument
    {
        public int SchemaVersion { get; set; }

        public List<NodeDocument>? Nodes { get; set; }

        public List<ConnectionDocument>? Connections { get; set; }
    }

    internal sealed class NodeDocument
    {
        public string Id { get; set; } = string.Empty;

        public LogicNodeKind Kind { get; set; }

        public string Label { get; set; } = string.Empty;

        public double X { get; set; }

        public double Z { get; set; }

        public string? DefinitionId { get; set; }

        public LogicSignal InputSignal { get; set; }
    }

    internal sealed class ConnectionDocument
    {
        public string Id { get; set; } = string.Empty;

        public string SourceNodeId { get; set; } = string.Empty;

        public string SourcePort { get; set; } = string.Empty;

        public string DestinationNodeId { get; set; } = string.Empty;

        public string DestinationPort { get; set; } = string.Empty;
    }
}
