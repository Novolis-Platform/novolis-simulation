namespace Novolis.Simulation.Logic;

/// <summary>
/// A stable, user-presentable circuit diagnostic.
/// </summary>
public sealed record LogicDiagnostic
{
    /// <summary>Creates a diagnostic.</summary>
    public LogicDiagnostic(
        LogicDiagnosticCode code,
        LogicDiagnosticSeverity severity,
        string message,
        LogicNodeId? nodeId = null,
        LogicConnectionId? connectionId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        Code = code;
        Severity = severity;
        Message = message.Trim();
        NodeId = nodeId;
        ConnectionId = connectionId;
    }

    /// <summary>Gets the diagnostic code.</summary>
    public LogicDiagnosticCode Code { get; }

    /// <summary>Gets the diagnostic severity.</summary>
    public LogicDiagnosticSeverity Severity { get; }

    /// <summary>Gets the human-readable message.</summary>
    public string Message { get; }

    /// <summary>Gets the related node, if any.</summary>
    public LogicNodeId? NodeId { get; }

    /// <summary>Gets the related connection, if any.</summary>
    public LogicConnectionId? ConnectionId { get; }
}
