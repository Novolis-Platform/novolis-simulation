namespace Novolis.Simulation.Logic;

/// <summary>
/// Immutable circuit node data.
/// </summary>
public sealed record LogicNode
{
    /// <summary>Creates a circuit node.</summary>
    public LogicNode(
        LogicNodeId id,
        LogicNodeKind kind,
        string? label = null,
        double x = 0,
        double z = 0,
        string? definitionId = null,
        LogicValue? inputValue = null)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException("A node requires a non-empty identity.", nameof(id));

        Id = id;
        Kind = kind;
        Label = string.IsNullOrWhiteSpace(label) ? kind.ToString() : label.Trim();
        X = x;
        Z = z;
        DefinitionId = string.IsNullOrWhiteSpace(definitionId)
            ? null
            : definitionId.Trim();
        InputValue = inputValue ?? LogicValue.Disconnected;
    }

    /// <summary>Gets the stable node identity.</summary>
    public LogicNodeId Id { get; }

    /// <summary>Gets the node kind.</summary>
    public LogicNodeKind Kind { get; }

    /// <summary>Gets the display label.</summary>
    public string Label { get; }

    /// <summary>Gets the planar X coordinate.</summary>
    public double X { get; }

    /// <summary>Gets the planar Z coordinate.</summary>
    public double Z { get; }

    /// <summary>Gets the custom-chip definition identity, if applicable.</summary>
    public string? DefinitionId { get; }

    /// <summary>Gets the configured value used by an input node.</summary>
    public LogicValue InputValue { get; }

    /// <summary>Creates an input node.</summary>
    public static LogicNode CreateInput(
        string label = "Input",
        double x = 0,
        double z = 0,
        LogicValue? value = null) =>
        new(LogicNodeId.New(), LogicNodeKind.Input, label, x, z, inputValue: value);

    /// <summary>Creates an output node.</summary>
    public static LogicNode CreateOutput(
        string label = "Output",
        double x = 0,
        double z = 0) =>
        new(LogicNodeId.New(), LogicNodeKind.Output, label, x, z);

    /// <summary>Creates a built-in gate node.</summary>
    public static LogicNode CreateGate(
        LogicNodeKind kind,
        string? label = null,
        double x = 0,
        double z = 0)
    {
        if (kind is LogicNodeKind.Input or LogicNodeKind.Output or LogicNodeKind.CustomChip)
        {
            throw new ArgumentException(
                "CreateGate accepts only built-in gate kinds.",
                nameof(kind));
        }

        return new(LogicNodeId.New(), kind, label, x, z);
    }

    /// <summary>Creates an instance of a saved custom chip.</summary>
    public static LogicNode CreateCustomChip(
        string definitionId,
        string? label = null,
        double x = 0,
        double z = 0) =>
        new(
            LogicNodeId.New(),
            LogicNodeKind.CustomChip,
            label,
            x,
            z,
            definitionId);

    /// <summary>Returns a copy at a new planar position.</summary>
    public LogicNode WithPosition(double x, double z) =>
        new(Id, Kind, Label, x, z, DefinitionId, InputValue);

    /// <summary>Returns a copy with a new configured input value.</summary>
    public LogicNode WithInputValue(LogicValue value) =>
        new(Id, Kind, Label, X, Z, DefinitionId, value);
}
