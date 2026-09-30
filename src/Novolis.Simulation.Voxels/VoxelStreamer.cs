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
