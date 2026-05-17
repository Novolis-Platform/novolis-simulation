namespace Novolis.Simulation.World;

/// <summary>Axis-aligned interior volume for sphere centers inside an occupancy room (+Y up).</summary>
public readonly struct RoomInteriorBounds
{
    public float MinX { get; init; }
    public float MaxX { get; init; }
    public float MinY { get; init; }
    public float MaxY { get; init; }
    public float MinZ { get; init; }
    public float MaxZ { get; init; }

    /// <summary>Inset AABB for sphere centers from grid size, cell size, wall height, and sphere radius.</summary>
    public static RoomInteriorBounds ForOccupancyGrid(
        uint gridWidth,
        uint gridHeight,
        float cellSize,
        float wallHeight,
        float sphereRadius)
    {
        var wallInset = cellSize + sphereRadius + 0.04f;
        var maxX = (gridWidth - 1) * cellSize - sphereRadius - 0.04f;
        var maxZ = (gridHeight - 1) * cellSize - sphereRadius - 0.04f;
        return new RoomInteriorBounds
        {
            MinX = wallInset,
            MaxX = maxX,
            MinY = sphereRadius,
            MaxY = wallHeight - sphereRadius,
            MinZ = wallInset,
            MaxZ = maxZ,
        };
    }
}
