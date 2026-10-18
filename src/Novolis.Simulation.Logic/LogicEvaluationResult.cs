using System.Collections.ObjectModel;

namespace Novolis.Simulation.Logic;

/// <summary>
/// Snapshot produced by evaluating one circuit tick.
/// </summary>
public sealed class LogicEvaluationResult
{
    /// <summary>Creates an evaluation result.</summary>
    public LogicEvaluationResult(
        IReadOnlyDictionary<LogicPortAddress, LogicValue>? portValues = null,
        IReadOnlyDictionary<LogicNodeId, LogicValue>? outputValues = null,
        IEnumerable<LogicDiagnostic>? diagnostics = null)
    {
        PortValues = new ReadOnlyDictionary<LogicPortAddress, LogicValue>(
            new Dictionary<LogicPortAddress, LogicValue>(
                portValues ?? new Dictionary<LogicPortAddress, LogicValue>()));
        OutputValues = new ReadOnlyDictionary<LogicNodeId, LogicValue>(
            new Dictionary<LogicNodeId, LogicValue>(
                outputValues ?? new Dictionary<LogicNodeId, LogicValue>()));
        Diagnostics = new ReadOnlyCollection<LogicDiagnostic>(
            (diagnostics ?? []).ToArray());
    }

    /// <summary>Gets values observed at evaluated ports.</summary>
    public IReadOnlyDictionary<LogicPortAddress, LogicValue> PortValues { get; }

    /// <summary>Gets values observed at output nodes.</summary>
    public IReadOnlyDictionary<LogicNodeId, LogicValue> OutputValues { get; }

    /// <summary>Gets diagnostics produced during evaluation.</summary>
    public IReadOnlyList<LogicDiagnostic> Diagnostics { get; }

    /// <summary>Gets whether evaluation completed without error diagnostics.</summary>
    public bool IsValid =>
        Diagnostics.All(diagnostic =>
            diagnostic.Severity != LogicDiagnosticSeverity.Error);

    /// <summary>Gets the value at a port, or disconnected when it was not evaluated.</summary>
    public LogicValue GetValue(LogicPortAddress address) =>
        PortValues.TryGetValue(address, out var value)
            ? value
            : LogicValue.Disconnected;
}
