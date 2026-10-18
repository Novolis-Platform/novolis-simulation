using System.Collections.ObjectModel;

namespace Novolis.Simulation.Logic;

/// <summary>
/// A named circuit that can be placed as a reusable custom node.
/// </summary>
public sealed class LogicChipDefinition
{
    /// <summary>Creates a custom-chip definition.</summary>
    public LogicChipDefinition(
        string id,
        LogicCircuit circuit,
        IReadOnlyDictionary<string, LogicNodeId> inputNodes,
        IReadOnlyDictionary<string, LogicNodeId> outputNodes)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(circuit);
        ArgumentNullException.ThrowIfNull(inputNodes);
        ArgumentNullException.ThrowIfNull(outputNodes);

        Id = id.Trim();
        Circuit = circuit;
        InputNodes = CopyMap(inputNodes, nameof(inputNodes));
        OutputNodes = CopyMap(outputNodes, nameof(outputNodes));
        Ports = new ReadOnlyCollection<LogicPortDefinition>(
        [
            .. InputNodes.Keys
                .Order(StringComparer.Ordinal)
                .Select(name => new LogicPortDefinition(
                    name,
                    LogicPortDirection.Input)),
            .. OutputNodes.Keys
                .Order(StringComparer.Ordinal)
                .Select(name => new LogicPortDefinition(
                    name,
                    LogicPortDirection.Output)),
        ]);
    }

    /// <summary>Gets the stable definition identity.</summary>
    public string Id { get; }

    /// <summary>Gets the internal circuit.</summary>
    public LogicCircuit Circuit { get; }

    /// <summary>Gets external input names and their internal input nodes.</summary>
    public IReadOnlyDictionary<string, LogicNodeId> InputNodes { get; }

    /// <summary>Gets external output names and their internal output nodes.</summary>
    public IReadOnlyDictionary<string, LogicNodeId> OutputNodes { get; }

    /// <summary>Gets the externally visible port contract.</summary>
    public IReadOnlyList<LogicPortDefinition> Ports { get; }

    private static IReadOnlyDictionary<string, LogicNodeId> CopyMap(
        IReadOnlyDictionary<string, LogicNodeId> source,
        string parameterName)
    {
        var copy = new Dictionary<string, LogicNodeId>(StringComparer.Ordinal);
        foreach (var pair in source)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pair.Key);
            if (pair.Value.Value == Guid.Empty)
            {
                throw new ArgumentException(
                    "Chip port mappings require non-empty node identities.",
                    parameterName);
            }

            if (!copy.TryAdd(pair.Key.Trim(), pair.Value))
            {
                throw new ArgumentException(
                    $"Chip port '{pair.Key}' occurs more than once.",
                    parameterName);
            }
        }

        return new ReadOnlyDictionary<string, LogicNodeId>(copy);
    }
}
