namespace Novolis.Simulation.Tiles;

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
