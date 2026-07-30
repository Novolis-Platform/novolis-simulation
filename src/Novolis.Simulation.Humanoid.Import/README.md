# Novolis.Simulation.Humanoid.Import

BVH mocap and lightweight glTF joint import into `Novolis.Simulation.Humanoid` clips.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Import
```

## BVH

```csharp
var clip = BvhHumanoidImporter.ImportFile("walk.bvh"); // cm→m by default
var pose = new HumanoidPose();
clip.Sample(0.5f, pose, HumanoidBindPose.CreateDefaultTPose());
```

## glTF (bind frame)

```csharp
var clip = GltfHumanoidImporter.ImportBindPoseFile("character.gltf");
```

Full glTF animation sampler playback and Assimp FBX stay follow-ons; joint names must match Mixamo/Unity aliases (`HumanoidBoneNames`).
