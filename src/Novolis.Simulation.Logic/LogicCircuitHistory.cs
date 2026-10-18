namespace Novolis.Simulation.Logic;

/// <summary>
/// Host-neutral undo/redo history for immutable circuit snapshots.
/// </summary>
public sealed class LogicCircuitHistory
{
    private readonly Stack<LogicCircuit> _undo = [];
    private readonly Stack<LogicCircuit> _redo = [];

    /// <summary>Creates history rooted at a circuit snapshot.</summary>
    public LogicCircuitHistory(LogicCircuit? initial = null)
    {
        Current = initial ?? LogicCircuit.Empty;
    }

    /// <summary>Gets the current circuit snapshot.</summary>
    public LogicCircuit Current { get; private set; }

    /// <summary>Gets whether an undo operation is available.</summary>
    public bool CanUndo => _undo.Count > 0;

    /// <summary>Gets whether a redo operation is available.</summary>
    public bool CanRedo => _redo.Count > 0;

    /// <summary>Applies one edit and records the previous snapshot.</summary>
    public LogicCircuit Execute(Func<LogicCircuit, LogicCircuit> edit)
    {
        ArgumentNullException.ThrowIfNull(edit);
        var next = edit(Current)
                   ?? throw new InvalidOperationException(
                       "A circuit edit must return a snapshot.");
        if (ReferenceEquals(next, Current))
            return Current;

        _undo.Push(Current);
        Current = next;
        _redo.Clear();
        return Current;
    }

    /// <summary>Undoes the most recent edit.</summary>
    public bool Undo()
    {
        if (_undo.Count == 0)
            return false;

        _redo.Push(Current);
        Current = _undo.Pop();
        return true;
    }

    /// <summary>Redoes the most recently undone edit.</summary>
    public bool Redo()
    {
        if (_redo.Count == 0)
            return false;

        _undo.Push(Current);
        Current = _redo.Pop();
        return true;
    }

    /// <summary>Clears edit history while retaining the current snapshot.</summary>
    public void Clear()
    {
        _undo.Clear();
        _redo.Clear();
    }

    /// <summary>Replaces the current snapshot and clears edit history.</summary>
    public void Reset(LogicCircuit? circuit = null)
    {
        Current = circuit ?? LogicCircuit.Empty;
        Clear();
    }
}
