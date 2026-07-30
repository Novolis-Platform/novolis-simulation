namespace Novolis.Simulation.Tiles;

/// <summary>Axis-aligned dirty region in cell space (inclusive min, exclusive max).</summary>
public readonly record struct DirtyRect(int MinX, int MinZ, int MaxX, int MaxZ)
{
    /// <summary>Empty rect.</summary>
    public static DirtyRect Empty { get; } = new(0, 0, 0, 0);

    /// <summary>Whether the rect has area.</summary>
    public bool IsEmpty => MaxX <= MinX || MaxZ <= MinZ;

    /// <summary>Union of two rects (empty absorbs).</summary>
    public static DirtyRect Union(DirtyRect a, DirtyRect b)
    {
        if (a.IsEmpty) return b;
        if (b.IsEmpty) return a;
        return new DirtyRect(
            System.Math.Min(a.MinX, b.MinX),
            System.Math.Min(a.MinZ, b.MinZ),
            System.Math.Max(a.MaxX, b.MaxX),
            System.Math.Max(a.MaxZ, b.MaxZ));
    }

    /// <summary>Single-cell dirty region.</summary>
    public static DirtyRect Cell(int x, int z) => new(x, z, x + 1, z + 1);
}

/// <summary>Accumulates place/demolish ops and a dirty AABB.</summary>
public sealed class BuildBatch
{
    /// <summary>Creates a batch for a map of the given size.</summary>
    public BuildBatch(int width, int depth)
    {
        Width = width;
        Depth = depth;
    }

    /// <summary>Map width.</summary>
    public int Width { get; }

    /// <summary>Map depth.</summary>
    public int Depth { get; }

    /// <summary>Accumulated dirty region.</summary>
    public DirtyRect Dirty { get; private set; } = DirtyRect.Empty;

    /// <summary>Marks a cell dirty.</summary>
    public void TouchCell(int x, int z)
    {
        if ((uint)x >= Width || (uint)z >= Depth)
            return;
        Dirty = DirtyRect.Union(Dirty, DirtyRect.Cell(x, z));
    }

    /// <summary>Marks an inclusive area dirty.</summary>
    public void TouchRect(int minX, int minZ, int maxXExclusive, int maxZExclusive)
    {
        Dirty = DirtyRect.Union(Dirty, new DirtyRect(minX, minZ, maxXExclusive, maxZExclusive));
    }

    /// <summary>Clears dirty state after remesh/path rebuild.</summary>
    public void ClearDirty() => Dirty = DirtyRect.Empty;
}
