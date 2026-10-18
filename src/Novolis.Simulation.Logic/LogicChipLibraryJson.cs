using System.Text.Json;

namespace Novolis.Simulation.Logic;

/// <summary>
/// Versioned JSON persistence for reusable custom-chip definitions.
/// </summary>
public static class LogicChipLibraryJson
{
    /// <summary>Serializes a chip library and each nested circuit.</summary>
    public static string Serialize(LogicChipLibrary library, bool indented = true)
    {
        ArgumentNullException.ThrowIfNull(library);
        var document = new LibraryDocument
        {
            SchemaVersion = LogicCircuit.CurrentSchemaVersion,
            Definitions = library.Definitions
                .Select(definition =>
                {
                    using var circuitDocument = JsonDocument.Parse(
                        LogicCircuitJson.Serialize(definition.Circuit, indented: false));
                    return new DefinitionDocument
                    {
                        Id = definition.Id,
                        Circuit = circuitDocument.RootElement.Clone(),
                        InputNodes = definition.InputNodes.ToDictionary(
                            pair => pair.Key,
                            pair => pair.Value.ToString(),
                            StringComparer.Ordinal),
                        OutputNodes = definition.OutputNodes.ToDictionary(
                            pair => pair.Key,
                            pair => pair.Value.ToString(),
                            StringComparer.Ordinal),
                    };
                })
                .ToList(),
        };

        var options = new JsonSerializerOptions(
            LogicCircuitJson.SerializerOptions)
        {
            WriteIndented = indented,
        };
        return JsonSerializer.Serialize(document, options);
    }

    /// <summary>Deserializes a reusable chip library.</summary>
    public static LogicChipLibrary Deserialize(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        var document = JsonSerializer.Deserialize<LibraryDocument>(
                           json,
                           LogicCircuitJson.SerializerOptions)
                       ?? throw new JsonException("The chip library was empty.");
        if (document.SchemaVersion != LogicCircuit.CurrentSchemaVersion)
        {
            throw new JsonException(
                $"Unsupported chip library schema version {document.SchemaVersion}.");
        }

        try
        {
            var definitions = (document.Definitions ?? [])
                .Select(definition => new LogicChipDefinition(
                    definition.Id,
                    LogicCircuitJson.Deserialize(
                        definition.Circuit.GetRawText()),
                    ParseMap(definition.InputNodes),
                    ParseMap(definition.OutputNodes)))
                .ToArray();
            return new LogicChipLibrary(definitions);
        }
        catch (FormatException exception)
        {
            throw new JsonException(
                "The chip library contains an invalid identity.",
                exception);
        }
        catch (ArgumentException exception)
        {
            throw new JsonException(
                "The chip library contains an invalid value.",
                exception);
        }
    }

    private static IReadOnlyDictionary<string, LogicNodeId> ParseMap(
        Dictionary<string, string>? values)
    {
        var result = new Dictionary<string, LogicNodeId>(
            StringComparer.Ordinal);
        foreach (var pair in values ?? [])
            result[pair.Key] = LogicNodeId.Parse(pair.Value);
        return result;
    }

    internal sealed class LibraryDocument
    {
        public int SchemaVersion { get; set; }

        public List<DefinitionDocument>? Definitions { get; set; }
    }

    internal sealed class DefinitionDocument
    {
        public string Id { get; set; } = string.Empty;

        public JsonElement Circuit { get; set; }

        public Dictionary<string, string>? InputNodes { get; set; }

        public Dictionary<string, string>? OutputNodes { get; set; }
    }
}
