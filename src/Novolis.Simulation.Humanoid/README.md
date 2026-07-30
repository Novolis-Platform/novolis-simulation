# Novolis.Simulation.Humanoid

Cinema 4D / Unity / Mixamo–style **standard biped** for Novolis: named bones, T-pose bind, FK pose solve, two-bone IK, and animation clips. BCL-only (`System.Numerics`).

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid
```

## Quick start

```csharp
using Novolis.Simulation.Humanoid;
using System.Numerics;

var bind = HumanoidBindPose.CreateDefaultTPose(1.8f);
var pose = HumanoidPose.FromBind(bind);
pose[HumanoidBone.LeftArm] = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.4f);

var world = HumanoidPoseSolver.SolveWorld(bind, pose);
var sticks = HumanoidDebugDraw.BuildSegments(world);

// Limb IK
var mid = TwoBoneIk.SolveMid(
    world.Position(HumanoidBone.LeftArm),
    world.Position(HumanoidBone.LeftHand),
    upperLength: bind.BoneLength(HumanoidBone.LeftForeArm),
    lowerLength: bind.BoneLength(HumanoidBone.LeftHand),
    poleVector: Vector3.UnitZ);
```

## Bone standard

Mixamo / Unity Humanoid names (`HumanoidBone`, `HumanoidBoneNames.TryResolve`). Hierarchy under `HumanoidHierarchy`. Map to physics ragdoll spheres via `HumanoidRagdollMap` (indexes match `RagdollHumanoidPreset`).

## Not in this package

- Mesh skinning / GPU draw (apps / Rendering)
- BVH / FBX / glTF import (follow-on)
- Finger / face bones (later)
- Game-specific clip banks (`Novolis.Game.Humanoid` optional later)

## Related packages

| Package | Role |
|---------|------|
| `Novolis.Simulation.Humanoid.Physics` | Ragdoll sphere bridge |
| `Novolis.Simulation.Humanoid.Import` | BVH / glTF joint import |
| `Novolis.Simulation.Humanoid.Skinning` | CPU LBS deform |
| `Novolis.Game.Humanoid` | Clip banks / body masks |
