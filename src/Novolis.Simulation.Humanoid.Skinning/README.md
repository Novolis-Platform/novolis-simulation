# Novolis.Simulation.Humanoid.Skinning

CPU linear-blend skinning for `Novolis.Simulation.Humanoid` over `Novolis.Math.Geometry.TriangleMesh`.

GPU skinning stays in apps / Rendering backends.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Skinning
```

## Quick start

```csharp
var bind = HumanoidBindPose.CreateDefaultTPose();
var mesh = new TriangleMesh(vertices, indices);
var weights = /* per-vertex VertexBoneWeight[] */;
var skin = new SkinnedHumanoidMesh(mesh, weights, SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind));
var world = HumanoidPoseSolver.SolveWorld(bind, pose);
var deformed = CpuSkinDeformer.DeformToMesh(skin, world);
```
