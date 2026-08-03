<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.Voxels

Chunked voxel world (16³ `ushort` blocks), streaming, terrain fill, dig/place — storage for Minecraft-clone games.

## Install

```bash
dotnet add package Novolis.Simulation.Voxels
```

## Quick start

```csharp
using Novolis.Simulation.Voxels;

var world = new ChunkedVoxelWorld();
world.TrySetBlock(3, 1, 4, blockId: 1);

var streamer = new VoxelStreamer(world, radius: 2);
streamer.Update(playerX, playerY, playerZ);

TerrainFiller.FillChunk(world.GetOrCreateChunk(new ChunkCoord3(0, 0, 0)), (x, z) => 8);
```

Block storage types live in `Novolis.Math.Arrays` (`VoxelChunk`, `ChunkCoord3`).

## API

| Type | Role |
|------|------|
| `ChunkedVoxelWorld` | `GetOrCreateChunk`, `TryGetBlock`, `TrySetBlock`, `IsSolid`, dirty tracking |
| `VoxelStreamer` | `Update(focus)`, `ChunkNeeded`/`ChunkUnloaded` events |
| `TerrainFiller` | `FillChunk`, `FillWorld` height callbacks |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Voxels.Meshing` | Greedy/face-culled mesh generation |
| `Novolis.Math.Arrays` | `VoxelChunk`, `ChunkCoord3` types |

