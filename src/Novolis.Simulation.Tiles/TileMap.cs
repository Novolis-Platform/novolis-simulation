namespace Novolis.Simulation.Tiles;

/// <summary>Named tile layer ids for build maps.</summary>
public enum TileLayerKind : byte
{
    Floor = 0,
    Object = 1,
    Zone = 2
}

/// <summary>Multi-layer cell map on XZ (width × depth). Cell values are opaque ushort ids (0 = empty).</summary>
public sealed class TileMap2D
{
    readonly Dictionary<TileLayerKind, ushort[,]> _layers = new();

    /// <summary>Creates an empty map.</summary>
    public TileMap2D(int width, int depth)
    {
        if (width <= 0 || depth <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        Width = width;
        Depth = depth;
        foreach (TileLayerKind kind in Enum.GetValues<TileLayerKind>())
            _layers[kind] = new ushort[width, depth];
    }

    /// <summary>Cell count along +X.</summary>
    public int Width { get; }

    /// <summary>Cell count along +Z.</summary>
    public int Depth { get; }

    /// <summary>Gets a cell id (0 if empty).</summary>
    public ushort Get(TileLayerKind kind, int x, int z)
    {
        Validate(x, z);
        return _layers[kind][x, z];
    }

    /// <summary>Sets a cell id (0 clears).</summary>
    public void Set(TileLayerKind kind, int x, int z, ushort id)
    {
        Validate(x, z);
        _layers[kind][x, z] = id;
    }

    void Validate(int x, int z)
    {
        if ((uint)x >= Width || (uint)z >= Depth)
            throw new ArgumentOutOfRangeException($"Cell ({x},{z}) outside 0..{Width - 1},0..{Depth - 1}.");
    }
}

/// <summary>Axis for wall edges on the grid.</summary>
public enum WallAxis : byte
{
    /// <summary>Edge parallel to X (separates Z and Z+1).</summary>
    AlongX = 0,

    /// <summary>Edge parallel to Z (separates X and X+1).</summary>
    AlongZ = 1
}

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

/// <summary>
/// Prison Architect–style walls on cell edges.
/// H-edges: (width)×(depth+1) along X between Z rows; V-edges: (width+1)×(depth) along Z between X cols.
/// </summary>
public sealed class WallEdgeMap
{
    readonly WallEdge[,] _h;
    readonly WallEdge[,] _v;

    /// <summary>Creates an empty edge map for a <paramref name="width"/>×<paramref name="depth"/> cell grid.</summary>
    public WallEdgeMap(int width, int depth)
    {
        if (width <= 0 || depth <= 0)
            throw new ArgumentOutOfRangeException(nameof(width));
        Width = width;
        Depth = depth;
        _h = new WallEdge[width, depth + 1];
        _v = new WallEdge[width + 1, depth];
    }

    /// <summary>Cell width.</summary>
    public int Width { get; }

    /// <summary>Cell depth.</summary>
    public int Depth { get; }

    /// <summary>Horizontal edge at the south of cell (x, z) when <paramref name="zLine"/> == z, or north when zLine == z+1.</summary>
    public WallEdge GetH(int x, int zLine)
    {
        if ((uint)x >= Width || (uint)zLine > Depth)
            throw new ArgumentOutOfRangeException();
        return _h[x, zLine];
    }

    /// <summary>Sets a horizontal edge.</summary>
    public void SetH(int x, int zLine, WallEdge edge)
    {
        if ((uint)x >= Width || (uint)zLine > Depth)
            throw new ArgumentOutOfRangeException();
        _h[x, zLine] = edge;
    }

    /// <summary>Vertical edge at the west of cell (x, z) when <paramref name="xLine"/> == x.</summary>
    public WallEdge GetV(int xLine, int z)
    {
        if ((uint)xLine > Width || (uint)z >= Depth)
            throw new ArgumentOutOfRangeException();
        return _v[xLine, z];
    }

    /// <summary>Sets a vertical edge.</summary>
    public void SetV(int xLine, int z, WallEdge edge)
    {
        if ((uint)xLine > Width || (uint)z >= Depth)
            throw new ArgumentOutOfRangeException();
        _v[xLine, z] = edge;
    }

    /// <summary>True if travel from (x,z) to neighbor is blocked by an edge wall.</summary>
    public bool BlocksStep(int x, int z, int nx, int nz)
    {
        if (nx == x && nz == z + 1)
            return GetH(x, z + 1).Blocks;
        if (nx == x && nz == z - 1)
            return GetH(x, z).Blocks;
        if (nz == z && nx == x + 1)
            return GetV(x + 1, z).Blocks;
        if (nz == z && nx == x - 1)
            return GetV(x, z).Blocks;
        return true;
    }
}
