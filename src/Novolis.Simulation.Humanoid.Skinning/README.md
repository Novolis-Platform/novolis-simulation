# Novolis.Simulation.Humanoid.Skinning

CPU linear-blend skinning for `Novolis.Simulation.Humanoid` over `Novolis.Math.Geometry.TriangleMesh`,
plus **AdaptiveMesh** person hulls that follow ragdoll sphere graphs as one body surface.

GPU skinning stays in apps / Rendering backends.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Skinning
```

## Unrigged FBX → animatable (auto-skin)

```csharp
using Novolis.Math.Geometry;
using Novolis.Modeling.Import;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Skinning;

var bind = HumanoidBindPose.CreateDefaultTPose(1.72f);
var raw = AssimpMeshImporter.ImportFile("character.fbx", new MeshImportOptions {
    PreTransformVertices = true,
    GenerateNormals = true,
});
var lod = MeshLod.Decimate(raw, targetTriangleCount: 20_000);
var aligned = HumanoidMeshAligner.FitToBindPose(lod, bind);
var skin = HumanoidNearestBoneSkinner.Bind(aligned, bind, influences: 4);

// each frame:
var world = HumanoidPoseSolver.SolveWorld(bind, pose);
var deformed = CpuSkinDeformer.DeformToMesh(skin, world);
```

When the file already has Mixamo-style bones, prefer Assimp named weights via
`AssimpSkinnedMeshImporter.TryImport` + `HumanoidNearestBoneSkinner.TryBindNamedWeights`.

## AdaptiveMesh hull

```csharp
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
| `HumanoidMeshAligner` | Fit unrigged mesh to bind height / feet |
| `HumanoidNearestBoneSkinner` | Auto-skin + Mixamo name map |
| `NamedBoneWeight` | Author bone-name influence |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Humanoid` | Poses and bind frames |
| `Novolis.Simulation.Humanoid.Physics` | Ragdoll sphere layout |
| `Novolis.Math.Geometry` | `MeshLod.Decimate` for realtime LODs |
| `Novolis.Modeling.Import` | Assimp geometry / named skin import |
