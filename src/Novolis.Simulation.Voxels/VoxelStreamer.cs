using Novolis.Math.Arrays;

namespace Novolis.Simulation.Voxels;

/// <summary>
/// Keeps a Chebyshev radius of chunks around a focus point loaded.
/// Raises load/unload so hosts can allocate meshes.
/// </summary>
public sealed class VoxelStreamer
{
    readonly ChunkedVoxelWorld _world;
    readonly HashSet<ChunkCoord3> _desired = [];

    /// <summary>Creates a streamer for <paramref name="world"/>.</summary>
    public VoxelStreamer(ChunkedVoxelWorld world, int radius = 2)
    {
        _world = world ?? throw new ArgumentNullException(nameof(world));
        Radius = System.Math.Max(0, radius);
    }

    /// <summary>Chebyshev radius in chunk units.</summary>
    public int Radius { get; set; }

    /// <summary>Fired when a chunk should be created/filled.</summary>
    public event Action<ChunkCoord3>? ChunkNeeded;

    /// <summary>Fired when a chunk leaves the window (after removal from world if present).</summary>
    public event Action<ChunkCoord3>? ChunkUnloaded;

    /// <summary>Updates the window around world-space focus (block units).</summary>
    public void Update(float focusX, float focusY, float focusZ)
    {
        const int s = VoxelChunk.Size;
        var cx = (int)MathF.Floor(focusX / s);
        var cy = (int)MathF.Floor(focusY / s);
        var cz = (int)MathF.Floor(focusZ / s);
        _desired.Clear();
        for (var dy = -Radius; dy <= Radius; dy++)
        for (var dz = -Radius; dz <= Radius; dz++)
        for (var dx = -Radius; dx <= Radius; dx++)
            _desired.Add(new ChunkCoord3(cx + dx, cy + dy, cz + dz));

        foreach (var coord in _desired)
        {
            if (_world.TryGetChunk(coord, out _))
                continue;
            ChunkNeeded?.Invoke(coord);
            _world.GetOrCreateChunk(coord);
        }

        List<ChunkCoord3>? remove = null;
        foreach (var kv in _world.Chunks)
        {
            if (_desired.Contains(kv.Key))
                continue;
            remove ??= [];
            remove.Add(kv.Key);
        }

        if (remove is null)
            return;

        foreach (var coord in remove)
        {
            _world.RemoveChunk(coord);
            ChunkUnloaded?.Invoke(coord);
        }
    }
}

/// <summary>Fills voxel columns from a height callback (world XZ → surface Y).</summary>
public static class TerrainFiller
{
    /// <summary>
    /// Fills blocks in <paramref name="chunk"/> for world columns covered by the chunk.
    /// Blocks from y=0..height-1 get <paramref name="blockId"/>; above is left as air.
    /// </summary>
    public static void FillChunk(
        VoxelChunk chunk,
        Func<int, int, int> sampleSurfaceY,
        ushort blockId = 1,
        int minY = 0)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentNullException.ThrowIfNull(sampleSurfaceY);
        var originX = chunk.Coord.X * VoxelChunk.Size;
        var originY = chunk.Coord.Y * VoxelChunk.Size;
        var originZ = chunk.Coord.Z * VoxelChunk.Size;

        for (var lz = 0; lz < VoxelChunk.Size; lz++)
        for (var lx = 0; lx < VoxelChunk.Size; lx++)
        {
            var wx = originX + lx;
            var wz = originZ + lz;
            var surface = sampleSurfaceY(wx, wz);
            for (var ly = 0; ly < VoxelChunk.Size; ly++)
            {
                var wy = originY + ly;
                if (wy >= minY && wy < surface)
                    chunk.Set(lx, ly, lz, blockId);
                else
                    chunk.Set(lx, ly, lz, 0);
            }
        }
    }

    /// <summary>Fills all currently loaded chunks in the world.</summary>
    public static void FillWorld(
        ChunkedVoxelWorld world,
        Func<int, int, int> sampleSurfaceY,
        ushort blockId = 1)
    {
        ArgumentNullException.ThrowIfNull(world);
        foreach (var chunk in world.Chunks.Values)
        {
            FillChunk(chunk, sampleSurfaceY, blockId);
            world.MarkDirty(chunk.Coord);
        }
    }
}
