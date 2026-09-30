namespace Novolis.Simulation.Tiles;

/// <summary>Wall/door state on one edge.</summary>
public readonly record struct WallEdge(bool Wall, bool Door)
{
    /// <summary>Blocks pathing when wall and not a door.</summary>
    public bool Blocks => Wall && !Door;

    /// <summary>Solid wall with no opening.</summary>
    public static WallEdge Solid { get; } = new(true, false);

    /// <summary>Wall segment with a door opening.</summary>
    public static WallEdge OpenDoor { get; } = new(true, true);

    /// <summary>No wall.</summary>
    public static WallEdge None { get; } = new(false, false);
}
