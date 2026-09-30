using Novolis.Math.Arrays;

namespace Novolis.Simulation.Tiles;

/// <summary>Builds a <see cref="DenseGrid{T}"/> occupancy (0 walkable, 1 blocked) for PlanarOccupancy.</summary>
public static class WalkabilityMask
{
    /// <summary>
    /// Marks a cell blocked when <paramref name="cellBlocked"/> is true, or when all four edges are solid walls
    /// (optional enclosure heuristic is not applied — only explicit cell blockers and object layer).
    /// </summary>
    public static DenseGrid<byte> FromBlockedCells(int width, int depth, Func<int, int, bool> cellBlocked)
    {
        ArgumentNullException.ThrowIfNull(cellBlocked);
        var grid = new DenseGrid<byte>((uint)width, 1, (uint)depth);
        for (var z = 0; z < depth; z++)
        for (var x = 0; x < width; x++)
            grid[(uint)x, 0, (uint)z] = cellBlocked(x, z) ? (byte)1 : (byte)0;
        return grid;
    }

    /// <summary>Blocks cells that have a non-zero object-layer tile.</summary>
    public static DenseGrid<byte> FromObjectLayer(TileMap2D map)
    {
        ArgumentNullException.ThrowIfNull(map);
        return FromBlockedCells(map.Width, map.Depth, (x, z) => map.Get(TileLayerKind.Object, x, z) != 0);
    }
}
