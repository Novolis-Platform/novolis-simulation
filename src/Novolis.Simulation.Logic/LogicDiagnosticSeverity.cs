namespace Novolis.Simulation.Logic;

/// <summary>
/// Severity of a logic diagnostic.
/// </summary>
public enum LogicDiagnosticSeverity
{
    /// <summary>Informational diagnostic.</summary>
    Info,
    /// <summary>Recoverable warning.</summary>
    Warning,
    /// <summary>Diagnostic that prevents valid evaluation.</summary>
    Error,
}
