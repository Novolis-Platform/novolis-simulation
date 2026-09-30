namespace Novolis.Simulation.Tiles;

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
