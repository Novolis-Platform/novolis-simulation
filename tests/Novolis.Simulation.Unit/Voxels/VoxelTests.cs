using Novolis.Math.Arrays;
using Novolis.Simulation.Voxels;
using Novolis.Simulation.Voxels.Meshing;

namespace Novolis.Simulation.Unit.Voxels;

public sealed class VoxelTests
{
    [Test]
    public async Task World_SetGet_Across_Chunk_Border()
    {
        var world = new ChunkedVoxelWorld();
        world.TrySetBlock(15, 0, 0, 3);
        world.TrySetBlock(16, 0, 0, 4);
        await Assert.That(world.GetBlock(15, 0, 0)).IsEqualTo((ushort)3);
        await Assert.That(world.GetBlock(16, 0, 0)).IsEqualTo((ushort)4);
        await Assert.That(world.Chunks.Count).IsEqualTo(2);
        await Assert.That(world.DirtyChunks.Count).IsGreaterThanOrEqualTo(2);
    }

    [Test]
    public async Task Streamer_Loads_And_Unloads()
    {
        var world = new ChunkedVoxelWorld();
        var streamer = new VoxelStreamer(world, radius: 1);
        var unloaded = 0;
        streamer.ChunkUnloaded += _ => unloaded++;
        streamer.Update(8f, 8f, 8f);
        var loaded = world.Chunks.Count;
        await Assert.That(loaded).IsEqualTo(27);
        streamer.Update(8f + VoxelChunk.Size * 10, 8f, 8f);
        await Assert.That(unloaded).IsGreaterThan(0);
        await Assert.That(world.Chunks.Count).IsEqualTo(27);
    }

    [Test]
    public async Task TerrainFiller_Produces_Surface()
    {
        var world = new ChunkedVoxelWorld();
        var chunk = world.GetOrCreateChunk(new ChunkCoord3(0, 0, 0));
        TerrainFiller.FillChunk(chunk, (_, _) => 5, blockId: 2);
        await Assert.That(chunk.Get(0, 4, 0)).IsEqualTo((ushort)2);
        await Assert.That(chunk.Get(0, 5, 0)).IsEqualTo((ushort)0);
    }

    [Test]
    public async Task FaceCulled_Emits_Faces_For_Single_Block()
    {
        var world = new ChunkedVoxelWorld();
        world.TrySetBlock(1, 1, 1, 1);
        world.ClearDirty();
        var mesh = FaceCulledMesher.Build(world, new ChunkCoord3(0, 0, 0));
        await Assert.That(mesh.TriangleCount).IsEqualTo(12); // 6 faces × 2 tris
    }

    [Test]
    public async Task Greedy_Fewer_Quads_Than_FaceCulled_On_Solid_Cube()
    {
        var world = new ChunkedVoxelWorld();
        var coord = new ChunkCoord3(0, 0, 0);
        var chunk = world.GetOrCreateChunk(coord);
        chunk.Fill(1);
        world.MarkDirty(coord);

        var face = FaceCulledMesher.Build(world, coord);
        var greedy = GreedyMesher.Build(world, coord);
        await Assert.That(greedy.TriangleCount).IsLessThan(face.TriangleCount);
        await Assert.That(greedy.TriangleCount).IsEqualTo(12); // 6 faces
    }
}
