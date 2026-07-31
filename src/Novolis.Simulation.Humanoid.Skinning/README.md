# Novolis.Simulation.Humanoid.Skinning

CPU linear-blend skinning for `Novolis.Simulation.Humanoid` over `Novolis.Math.Geometry.TriangleMesh`,
plus **AdaptiveMesh** person hulls that follow ragdoll sphere graphs as one body surface.

GPU skinning stays in apps / Rendering backends.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Skinning
```

## Adaptive person (ragdoll)

```csharp
var centers = /* 11 ragdoll sphere positions */;
var body = HumanoidAdaptiveBody.CreateFromRagdollBind(centers);
var handles = new Vector3[HumanoidAdaptiveBody.SphereCount];
HumanoidAdaptiveBody.CopySphereCenters(centers, handles);
var mesh = body.AdaptToMesh(handles); // single TriangleMesh following the doll
```

## Classic LBS skinning

```csharp
var bind = HumanoidBindPose.CreateDefaultTPose();
var mesh = new TriangleMesh(vertices, indices);
var weights = /* per-vertex VertexBoneWeight[] */;
var skin = new SkinnedHumanoidMesh(mesh, weights, SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind));
var world = HumanoidPoseSolver.SolveWorld(bind, pose);
var deformed = CpuSkinDeformer.DeformToMesh(skin, world);
```
