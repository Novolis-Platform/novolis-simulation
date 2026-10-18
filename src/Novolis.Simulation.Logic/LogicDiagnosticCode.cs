namespace Novolis.Simulation.Logic;

/// <summary>
/// Stable categories for circuit validation and evaluation diagnostics.
/// </summary>
public enum LogicDiagnosticCode
{
    /// <summary>A node identity occurs more than once.</summary>
    DuplicateNodeId,
    /// <summary>A connection identity occurs more than once.</summary>
    DuplicateConnectionId,
    /// <summary>A connection source node is missing.</summary>
    MissingSourceNode,
    /// <summary>A connection destination node is missing.</summary>
    MissingDestinationNode,
    /// <summary>A connection source port is missing.</summary>
    MissingSourcePort,
    /// <summary>A connection destination port is missing.</summary>
    MissingDestinationPort,
    /// <summary>A custom-chip definition is missing.</summary>
    MissingDefinition,
    /// <summary>A source port is not an output.</summary>
    SourceIsNotOutput,
    /// <summary>A destination port is not an input.</summary>
    DestinationIsNotInput,
    /// <summary>A single-input port has multiple drivers.</summary>
    InputCardinalityExceeded,
    /// <summary>A required input has no driver.</summary>
    RequiredInputUnconnected,
    /// <summary>The circuit contains a directed cycle.</summary>
    Cycle,
    /// <summary>Custom-chip definitions refer to themselves recursively.</summary>
    RecursiveDefinition,
    /// <summary>A custom-chip definition is structurally invalid.</summary>
    InvalidDefinition,
    /// <summary>Evaluation stopped because a cycle could not converge.</summary>
    NonConvergent,
}
