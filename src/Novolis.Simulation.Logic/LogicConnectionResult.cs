namespace Novolis.Simulation.Logic;

/// <summary>
/// Result of attempting to add a circuit connection.
/// </summary>
public sealed record LogicConnectionResult(
    bool IsValid,
    LogicCircuit Circuit,
    IReadOnlyList<LogicDiagnostic> Diagnostics);
