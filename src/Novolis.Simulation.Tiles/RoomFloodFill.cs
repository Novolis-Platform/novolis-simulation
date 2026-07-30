namespace Novolis.Simulation.Tiles;

/// <summary>Flood-fill enclosed regions from edge walls.</summary>
public static class RoomFloodFill
{
    /// <summary>
    /// Labels every cell with a room id (≥0). Open maps produce one region.
    /// Cells are 4-connected when the shared edge does not <see cref="WallEdge.Blocks"/>.
    /// </summary>
    public static int[,] LabelRooms(WallEdgeMap walls)
    {
        ArgumentNullException.ThrowIfNull(walls);
        var w = walls.Width;
        var d = walls.Depth;
        var labels = new int[w, d];
        for (var z = 0; z < d; z++)
        for (var x = 0; x < w; x++)
            labels[x, z] = -1;

        var next = 0;
        var queue = new Queue<(int X, int Z)>();
        for (var z = 0; z < d; z++)
        for (var x = 0; x < w; x++)
        {
            if (labels[x, z] >= 0)
                continue;
            var id = next++;
            labels[x, z] = id;
            queue.Enqueue((x, z));
            while (queue.Count > 0)
            {
                var (cx, cz) = queue.Dequeue();
                Try(cx + 1, cz);
                Try(cx - 1, cz);
                Try(cx, cz + 1);
                Try(cx, cz - 1);

                void Try(int nx, int nz)
                {
                    if ((uint)nx >= w || (uint)nz >= d)
                        return;
                    if (labels[nx, nz] >= 0)
                        return;
                    if (walls.BlocksStep(cx, cz, nx, nz))
                        return;
                    labels[nx, nz] = id;
                    queue.Enqueue((nx, nz));
                }
            }
        }

        return labels;
    }

    /// <summary>Number of distinct room ids in a label map.</summary>
    public static int CountRooms(int[,] labels)
    {
        var max = -1;
        var w = labels.GetLength(0);
        var d = labels.GetLength(1);
        for (var z = 0; z < d; z++)
        for (var x = 0; x < w; x++)
            max = System.Math.Max(max, labels[x, z]);
        return max + 1;
    }
}
