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
