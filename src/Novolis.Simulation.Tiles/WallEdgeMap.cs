namespace Novolis.Simulation.Tiles;

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
