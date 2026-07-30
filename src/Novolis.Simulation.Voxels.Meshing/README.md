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
var mesh = GreedyMesher.Build(world, new(0, 0, 0));
var tris = mesh.ToTriangleMesh();
```

No Rendering/Raylib references — apps upload the mesh to their GPU backend.
