<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-simulation/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-simulation/) · [Source](https://github.com/Novolis-Platform/novolis-simulation)
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.Humanoid.Import

BVH mocap and lightweight glTF joint import into `Novolis.Simulation.Humanoid` clips.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Import
```

## Quick start

```csharp
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Import;

var clip = BvhHumanoidImporter.ImportFile("walk.bvh"); // cm→m by default
var pose = new HumanoidPose();
clip.Sample(0.5f, pose, HumanoidBindPose.CreateDefaultTPose());
```

## glTF (bind frame)

```csharp
var bind = GltfHumanoidImporter.ImportBindPoseFile("character.gltf");
```

Joint names must match Mixamo/Unity aliases (`HumanoidBoneNames`). Full glTF animation sampler playback stays a follow-on.

## API

| Type | Role |
|------|------|
| `BvhHumanoidImporter` | `Import(bvhText, metersPerUnit?)`, `ImportFile(path, ...)` |
| `GltfHumanoidImporter` | `ImportBindPose(gltfJson, fallbackBind?)`, `ImportBindPoseFile(path, ...)` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Humanoid` | `HumanoidAnimationClip`, `HumanoidBindPose` |
| `Novolis.Game.Humanoid` | Locomotion clip banks and body masks |

