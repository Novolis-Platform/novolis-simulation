# Novolis.Simulation.Humanoid.Skinning

CPU linear-blend skinning for `Novolis.Simulation.Humanoid` over `Novolis.Math.Geometry.TriangleMesh`,
plus **AdaptiveMesh** person hulls that follow ragdoll sphere graphs as one body surface.

GPU skinning stays in apps / Rendering backends.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Skinning
```

## Quick start

```csharp
using Novolis.Simulation.Humanoid.Skinning;

var centers = /* 11 ragdoll sphere positions */;
var body = HumanoidAdaptiveBody.CreateFromRagdollBind(centers);
var handles = new Vector3[HumanoidAdaptiveBody.SphereCount];
HumanoidAdaptiveBody.CopySphereCenters(centers, handles);
var mesh = body.AdaptToMesh(handles);
```

## Classic LBS skinning

```csharp
var bind = HumanoidBindPose.CreateDefaultTPose();
var skin = new SkinnedHumanoidMesh(mesh, weights, SkinnedHumanoidMesh.CreateTranslationInverseBinds(bind));
var deformed = CpuSkinDeformer.DeformToMesh(skin, worldPose);
```

## API

| Type | Role |
|------|------|
| `VertexBoneWeight` | Per-vertex bone + weight |
| `SkinnedHumanoidMesh` | Bind mesh, weights, inverse binds |
| `CpuSkinDeformer` | `Deform`, `DeformToMesh` |
| `HumanoidAdaptiveBody` | `SphereCount=11`; `CreateFromRagdollBind`; `AdaptToMesh` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Humanoid` | Poses and bind frames |
| `Novolis.Simulation.Humanoid.Physics` | Ragdoll sphere layout |
