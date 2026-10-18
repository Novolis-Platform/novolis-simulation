using System.Collections.ObjectModel;

namespace Novolis.Simulation.Logic;

/// <summary>
/// Immutable registry of reusable custom-chip definitions.
/// </summary>
public sealed class LogicChipLibrary
{
    private readonly IReadOnlyDictionary<string, LogicChipDefinition> _definitions;

    /// <summary>Creates a validated chip library.</summary>
    public LogicChipLibrary(IEnumerable<LogicChipDefinition>? definitions = null)
        : this(definitions, validate: true)
    {
    }

    private LogicChipLibrary(
        IEnumerable<LogicChipDefinition>? definitions,
        bool validate)
    {
        var map = new Dictionary<string, LogicChipDefinition>(
            StringComparer.Ordinal);
        foreach (var definition in definitions ?? [])
        {
            ArgumentNullException.ThrowIfNull(definition);
            if (!map.TryAdd(definition.Id, definition))
            {
                throw new ArgumentException(
                    $"A chip definition named '{definition.Id}' occurs more than once.",
                    nameof(definitions));
            }
        }

        _definitions = new ReadOnlyDictionary<string, LogicChipDefinition>(map);
        Definitions = new ReadOnlyCollection<LogicChipDefinition>(
            map.Values.OrderBy(definition => definition.Id, StringComparer.Ordinal).ToArray());

        if (validate)
        {
            var result = Validate();
            if (!result.IsValid)
            {
                throw new InvalidOperationException(
                    string.Join(
                        Environment.NewLine,
                        result.Diagnostics.Select(diagnostic => diagnostic.Message)));
            }
        }
    }

    /// <summary>Gets an empty chip library.</summary>
    public static LogicChipLibrary Empty { get; } = new();

    /// <summary>Gets definitions in stable identity order.</summary>
    public IReadOnlyList<LogicChipDefinition> Definitions { get; }

    /// <summary>Looks up a definition by identity.</summary>
    public bool TryGet(string id, out LogicChipDefinition? definition)
    {
        if (!string.IsNullOrWhiteSpace(id) &&
            _definitions.TryGetValue(id.Trim(), out var value))
        {
            definition = value;
            return true;
        }

        definition = null;
        return false;
    }

    /// <summary>Returns a copy with one additional definition.</summary>
    public LogicChipLibrary Add(LogicChipDefinition definition)
    {
        if (!TryAdd(definition, out var library, out var diagnostics))
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    diagnostics.Select(diagnostic => diagnostic.Message)));
        }

        return library!;
    }

    /// <summary>
    /// Attempts to add a definition and returns diagnostics without mutating this library.
    /// </summary>
    public bool TryAdd(
        LogicChipDefinition definition,
        out LogicChipLibrary? library,
        out IReadOnlyList<LogicDiagnostic> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (_definitions.ContainsKey(definition.Id))
        {
            library = this;
            diagnostics =
            [
                new LogicDiagnostic(
                    LogicDiagnosticCode.InvalidDefinition,
                    LogicDiagnosticSeverity.Error,
                    $"Chip definition '{definition.Id}' already exists."),
            ];
            return false;
        }

        var candidate = new LogicChipLibrary(
            Definitions.Append(definition),
            validate: false);
        var validation = candidate.Validate();
        library = validation.IsValid ? candidate : this;
        diagnostics = validation.Diagnostics;
        return validation.IsValid;
    }

    /// <summary>Validates all definitions and their dependency graph.</summary>
    public LogicValidationResult Validate()
    {
        var diagnostics = new List<LogicDiagnostic>();
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (var definition in Definitions)
        {
            foreach (var diagnostic in ValidateDefinition(definition, this))
                diagnostics.Add(diagnostic);
            VisitDependencies(
                definition.Id,
                [],
                visiting,
                visited,
                diagnostics);
        }

        return new LogicValidationResult(SortDiagnostics(diagnostics));
    }

    private static IReadOnlyList<LogicDiagnostic> ValidateDefinition(
        LogicChipDefinition definition,
        LogicChipLibrary library)
    {
        var diagnostics = new List<LogicDiagnostic>();
        var circuitResult = definition.Circuit.Validate(library);
        foreach (var diagnostic in circuitResult.Diagnostics)
            diagnostics.Add(diagnostic);

        foreach (var pair in definition.InputNodes)
        {
            if (!definition.Circuit.TryGetNode(pair.Value, out var node))
            {
                diagnostics.Add(new LogicDiagnostic(
                    LogicDiagnosticCode.InvalidDefinition,
                    LogicDiagnosticSeverity.Error,
                    $"Chip '{definition.Id}' input '{pair.Key}' refers to missing node '{pair.Value}'."));
            }
            else if (node!.Kind != LogicNodeKind.Input)
            {
                diagnostics.Add(new LogicDiagnostic(
                    LogicDiagnosticCode.InvalidDefinition,
                    LogicDiagnosticSeverity.Error,
                    $"Chip '{definition.Id}' input '{pair.Key}' must refer to an input node.",
                    node.Id));
            }
        }

        foreach (var pair in definition.OutputNodes)
        {
            if (!definition.Circuit.TryGetNode(pair.Value, out var node))
            {
                diagnostics.Add(new LogicDiagnostic(
                    LogicDiagnosticCode.InvalidDefinition,
                    LogicDiagnosticSeverity.Error,
                    $"Chip '{definition.Id}' output '{pair.Key}' refers to missing node '{pair.Value}'."));
            }
            else if (node!.Kind != LogicNodeKind.Output)
            {
                diagnostics.Add(new LogicDiagnostic(
                    LogicDiagnosticCode.InvalidDefinition,
                    LogicDiagnosticSeverity.Error,
                    $"Chip '{definition.Id}' output '{pair.Key}' must refer to an output node.",
                    node.Id));
            }
        }

        return diagnostics;
    }

    private void VisitDependencies(
        string id,
        IReadOnlyList<string> path,
        ISet<string> visiting,
        ISet<string> visited,
        ICollection<LogicDiagnostic> diagnostics)
    {
        if (visiting.Contains(id))
        {
            diagnostics.Add(new LogicDiagnostic(
                LogicDiagnosticCode.RecursiveDefinition,
                LogicDiagnosticSeverity.Error,
                $"Recursive chip definition detected: {string.Join(" -> ", path.Append(id))}."));
            return;
        }

        if (!visited.Add(id) || !_definitions.TryGetValue(id, out var definition))
            return;

        visiting.Add(id);
        foreach (var node in definition.Circuit.Nodes
                     .Where(node => node.Kind == LogicNodeKind.CustomChip)
                     .OrderBy(node => node.Id))
        {
            if (node.DefinitionId is null ||
                !_definitions.ContainsKey(node.DefinitionId))
            {
                continue;
            }

            VisitDependencies(
                node.DefinitionId,
                path.Append(id).ToArray(),
                visiting,
                visited,
                diagnostics);
        }

        visiting.Remove(id);
    }

    private static IReadOnlyList<LogicDiagnostic> SortDiagnostics(
        IEnumerable<LogicDiagnostic> diagnostics) =>
        diagnostics
            .OrderBy(diagnostic => diagnostic.Code)
            .ThenBy(diagnostic => diagnostic.NodeId)
            .ThenBy(diagnostic => diagnostic.ConnectionId)
            .ThenBy(diagnostic => diagnostic.Message, StringComparer.Ordinal)
            .ToArray();
}
