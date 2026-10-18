namespace Novolis.Simulation.Logic;

/// <summary>
/// Immutable connection from one node output to another node input.
/// </summary>
public sealed record LogicConnection
{
    /// <summary>Creates a connection.</summary>
    public LogicConnection(
        LogicConnectionId id,
        LogicPortAddress source,
        LogicPortAddress destination)
    {
        if (id.Value == Guid.Empty)
            throw new ArgumentException(
                "A connection requires a non-empty identity.",
                nameof(id));

        Id = id;
        Source = source;
        Destination = destination;
    }

    /// <summary>Gets the stable connection identity.</summary>
    public LogicConnectionId Id { get; }

    /// <summary>Gets the source output address.</summary>
    public LogicPortAddress Source { get; }

    /// <summary>Gets the destination input address.</summary>
    public LogicPortAddress Destination { get; }
}
