using System.Collections.ObjectModel;

namespace Novolis.Simulation.Logic;

/// <summary>
/// The result of validating a circuit or chip library.
/// </summary>
public sealed class LogicValidationResult
{
    /// <summary>Creates a validation result.</summary>
    public LogicValidationResult(IEnumerable<LogicDiagnostic>? diagnostics = null)
    {
        Diagnostics = new ReadOnlyCollection<LogicDiagnostic>(
            (diagnostics ?? []).ToArray());
    }

    /// <summary>Gets validation diagnostics in deterministic order.</summary>
    public IReadOnlyList<LogicDiagnostic> Diagnostics { get; }

    /// <summary>Gets whether no error diagnostics were produced.</summary>
    public bool IsValid =>
        Diagnostics.All(diagnostic =>
            diagnostic.Severity != LogicDiagnosticSeverity.Error);
}
