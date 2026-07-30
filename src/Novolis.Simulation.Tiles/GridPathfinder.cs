using Novolis.Math.Arrays;

namespace Novolis.Simulation.Tiles;

/// <summary>A* pathfinding on a cell grid with edge walls and optional cell blockers.</summary>
public static class GridPathfinder
{
    static readonly (int Dx, int Dz)[] Ortho = [(1, 0), (-1, 0), (0, 1), (0, -1)];

    /// <summary>
    /// Finds a path from start to goal. Returns null if unreachable.
    /// <paramref name="blocked"/> cells (true = blocked) are impassable; edge walls use <paramref name="walls"/>.
    /// </summary>
    public static List<(int X, int Z)>? FindPath(
        WallEdgeMap walls,
        (int X, int Z) start,
        (int X, int Z) goal,
        bool[,]? blocked = null,
        bool allowDiagonal = false)
    {
        ArgumentNullException.ThrowIfNull(walls);
        var w = walls.Width;
        var d = walls.Depth;
        if (!In(start) || !In(goal))
            return null;
        if (blocked is not null && blocked[goal.X, goal.Z])
            return null;

        var open = new PriorityQueue<(int X, int Z), float>();
        var came = new Dictionary<(int, int), (int, int)>();
        var gScore = new Dictionary<(int, int), float> { [start] = 0f };
        open.Enqueue(start, Heuristic(start, goal));

        while (open.TryDequeue(out var current, out _))
        {
            if (current == goal)
                return Reconstruct(came, current);

            foreach (var (dx, dz) in Ortho)
            {
                var next = (current.X + dx, current.Z + dz);
                if (!In(next))
                    continue;
                if (blocked is not null && blocked[next.Item1, next.Item2])
                    continue;
                if (walls.BlocksStep(current.X, current.Z, next.Item1, next.Item2))
                    continue;

                var tentative = gScore[current] + 1f;
                if (gScore.TryGetValue(next, out var existing) && tentative >= existing)
                    continue;
                came[next] = current;
                gScore[next] = tentative;
                open.Enqueue(next, tentative + Heuristic(next, goal));
            }

            if (!allowDiagonal)
                continue;

            foreach (var (dx, dz) in new[] { (1, 1), (1, -1), (-1, 1), (-1, -1) })
            {
                var next = (current.X + dx, current.Z + dz);
                if (!In(next))
                    continue;
                if (blocked is not null && blocked[next.Item1, next.Item2])
                    continue;
                // Both ortho edges must be clear for diagonal.
                if (walls.BlocksStep(current.X, current.Z, current.X + dx, current.Z)
                    || walls.BlocksStep(current.X, current.Z, current.X, current.Z + dz))
                    continue;

                var tentative = gScore[current] + 1.41421356f;
                if (gScore.TryGetValue(next, out var existing) && tentative >= existing)
                    continue;
                came[next] = current;
                gScore[next] = tentative;
                open.Enqueue(next, tentative + Heuristic(next, goal));
            }
        }

        return null;

        bool In((int X, int Z) p) => (uint)p.X < w && (uint)p.Z < d;
    }

    static float Heuristic((int X, int Z) a, (int X, int Z) b) =>
        MathF.Abs(a.X - b.X) + MathF.Abs(a.Z - b.Z);

    static List<(int X, int Z)> Reconstruct(Dictionary<(int, int), (int, int)> came, (int X, int Z) current)
    {
        var path = new List<(int X, int Z)> { current };
        while (came.TryGetValue(current, out var prev))
        {
            current = prev;
            path.Add(current);
        }

        path.Reverse();
        return path;
    }
}

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
