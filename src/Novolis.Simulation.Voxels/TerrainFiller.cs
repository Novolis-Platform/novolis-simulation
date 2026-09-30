using Novolis.Math.Arrays;

namespace Novolis.Simulation.Voxels;

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
