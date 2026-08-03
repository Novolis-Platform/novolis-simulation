<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.Voxels.Meshing

Face-culled and greedy voxel meshing → `Novolis.Math.Geometry.EditableMesh` / `TriangleMesh`.

## Install

```bash
dotnet add package Novolis.Simulation.Voxels.Meshing
```

## Quick start

```csharp
using Novolis.Simulation.Voxels;
using Novolis.Simulation.Voxels.Meshing;

var world = new ChunkedVoxelWorld();
// ... fill blocks ...
var mesh = GreedyMesher.Build(world, new ChunkCoord3(0, 0, 0));
var tris = mesh.ToTriangleMesh();
```

No Rendering/Raylib references — apps upload the mesh to their GPU backend.

## API

| Type | Role |
|------|------|
| `GreedyMesher` | `Build(world, chunkCoord)` → merged quads as `EditableMesh` |
| `FaceCulledMesher` | `Build(...)`, `CountExposedFaces(...)` per-face meshing |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Voxels` | `ChunkedVoxelWorld` storage |
| `Novolis.Math.Geometry` | `EditableMesh`, `TriangleMesh` output |

