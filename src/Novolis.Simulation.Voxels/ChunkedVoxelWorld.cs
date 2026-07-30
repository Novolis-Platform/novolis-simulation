using Novolis.Math.Arrays;

namespace Novolis.Simulation.Voxels;

/// <summary>Chunked voxel world with dig/place and dirty tracking for remesh.</summary>
public sealed class ChunkedVoxelWorld
{
    readonly Dictionary<ChunkCoord3, VoxelChunk> _chunks = new();
    readonly HashSet<ChunkCoord3> _dirty = new();

    /// <summary>All loaded chunks.</summary>
    public IReadOnlyDictionary<ChunkCoord3, VoxelChunk> Chunks => _chunks;

    /// <summary>Chunks marked dirty since last <see cref="ClearDirty"/>.</summary>
    public IReadOnlyCollection<ChunkCoord3> DirtyChunks => _dirty;

    /// <summary>Gets or creates a chunk at <paramref name="coord"/>.</summary>
    public VoxelChunk GetOrCreateChunk(ChunkCoord3 coord)
    {
        if (_chunks.TryGetValue(coord, out var existing))
            return existing;
        var chunk = new VoxelChunk { Coord = coord };
        _chunks[coord] = chunk;
        return chunk;
    }

    /// <summary>Tries to get an existing chunk.</summary>
    public bool TryGetChunk(ChunkCoord3 coord, out VoxelChunk chunk) =>
        _chunks.TryGetValue(coord, out chunk!);

    /// <summary>Removes a chunk from the world.</summary>
    public bool RemoveChunk(ChunkCoord3 coord)
    {
        _dirty.Remove(coord);
        return _chunks.Remove(coord);
    }

    /// <summary>Maps world block coords to chunk + local.</summary>
    public static void WorldToLocal(int wx, int wy, int wz, out ChunkCoord3 coord, out int lx, out int ly, out int lz)
    {
        const int s = VoxelChunk.Size;
        var cx = FloorDiv(wx, s);
        var cy = FloorDiv(wy, s);
        var cz = FloorDiv(wz, s);
        coord = new ChunkCoord3(cx, cy, cz);
        lx = wx - cx * s;
        ly = wy - cy * s;
        lz = wz - cz * s;
    }

    /// <summary>Gets a block (0 if chunk missing).</summary>
    public ushort GetBlock(int wx, int wy, int wz)
    {
        WorldToLocal(wx, wy, wz, out var coord, out var lx, out var ly, out var lz);
        return _chunks.TryGetValue(coord, out var chunk) ? chunk.Get(lx, ly, lz) : (ushort)0;
    }

    /// <summary>True if block id is non-air.</summary>
    public bool IsSolid(int wx, int wy, int wz) => GetBlock(wx, wy, wz) != 0;

    /// <summary>
    /// Sets a block. Creates the chunk if needed. Marks this chunk and face-neighbors dirty when the value changes.
    /// </summary>
    public bool TrySetBlock(int wx, int wy, int wz, ushort id)
    {
        WorldToLocal(wx, wy, wz, out var coord, out var lx, out var ly, out var lz);
        var chunk = GetOrCreateChunk(coord);
        var prev = chunk.Set(lx, ly, lz, id);
        if (prev == id)
            return false;
        MarkDirty(coord);
        if (lx == 0) MarkDirty(new ChunkCoord3(coord.X - 1, coord.Y, coord.Z));
        if (lx == VoxelChunk.Size - 1) MarkDirty(new ChunkCoord3(coord.X + 1, coord.Y, coord.Z));
        if (ly == 0) MarkDirty(new ChunkCoord3(coord.X, coord.Y - 1, coord.Z));
        if (ly == VoxelChunk.Size - 1) MarkDirty(new ChunkCoord3(coord.X, coord.Y + 1, coord.Z));
        if (lz == 0) MarkDirty(new ChunkCoord3(coord.X, coord.Y, coord.Z - 1));
        if (lz == VoxelChunk.Size - 1) MarkDirty(new ChunkCoord3(coord.X, coord.Y, coord.Z + 1));
        return true;
    }

    /// <summary>Marks a chunk dirty for remesh (even if not loaded).</summary>
    public void MarkDirty(ChunkCoord3 coord) => _dirty.Add(coord);

    /// <summary>Clears dirty set after remesh.</summary>
    public void ClearDirty() => _dirty.Clear();

    static int FloorDiv(int a, int b)
    {
        var q = a / b;
        var r = a % b;
        if (r != 0 && ((r < 0) != (b < 0)))
            q--;
        return q;
    }
}
