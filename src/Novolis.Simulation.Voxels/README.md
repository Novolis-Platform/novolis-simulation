# Novolis.Simulation.Voxels

Chunked voxel world (16³ `ushort` blocks), streaming, terrain fill, dig/place — storage for Minecraft-clone games.

## Install

```bash
dotnet add package Novolis.Simulation.Voxels
```

## Quick start

```csharp
using Novolis.Math.Arrays;
using Novolis.Simulation.Voxels;

var world = new ChunkedVoxelWorld();
world.TrySetBlock(3, 1, 4, blockId: 1);

var streamer = new VoxelStreamer(world, radius: 2);
streamer.Update(playerX, playerY, playerZ);

TerrainFiller.FillChunk(world.GetOrCreateChunk(new ChunkCoord3(0, 0, 0)), (x, z) => 8);
```

Block storage types live in `Novolis.Math.Arrays` (`VoxelChunk`, `ChunkCoord3`). Meshing is `Novolis.Simulation.Voxels.Meshing`.
