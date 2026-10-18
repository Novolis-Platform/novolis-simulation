namespace Novolis.Simulation.Logic;

/// <summary>
/// The built-in or reusable kind of a circuit node.
/// </summary>
public enum LogicNodeKind
{
    /// <summary>External or configured input.</summary>
    Input,
    /// <summary>Observed circuit output.</summary>
    Output,
    /// <summary>Two-input conjunction.</summary>
    And,
    /// <summary>Two-input disjunction.</summary>
    Or,
    /// <summary>Single-input inversion.</summary>
    Not,
    /// <summary>Two-input exclusive disjunction.</summary>
    Xor,
    /// <summary>Two-input inverted conjunction.</summary>
    Nand,
    /// <summary>Reusable nested circuit.</summary>
    CustomChip,
}
