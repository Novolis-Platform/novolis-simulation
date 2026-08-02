# Novolis.Simulation.Humanoid

Cinema 4D / Unity / Mixamo–style **standard biped** for Novolis: named bones, T-pose bind, FK pose solve, two-bone IK, FABRIK chains, full-body multi-effector helpers, and animation clips. BCL-only (`System.Numerics`).

> **Not planar locomotion.** Floor agent move is **`Novolis.Simulation.Kinematics`** (`PlanarAgent`). Ragdoll dynamics are **`Novolis.Physics.Joints`** (bridge: `Humanoid.Physics`).

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
TwoBoneIk.ApplyLimb(
    world, bind,
    HumanoidBone.LeftArm, HumanoidBone.LeftForeArm, HumanoidBone.LeftHand,
    target: world.Position(HumanoidBone.LeftHand) + new Vector3(0.2f, 0.1f, 0f),
    upperLength: bind.BoneLength(HumanoidBone.LeftForeArm),
    lowerLength: bind.BoneLength(HumanoidBone.LeftHand),
    poleVector: Vector3.UnitZ);

// Multi-effector (hands / feet + optional head via spine FABRIK)
HumanoidFullBodyIk.Apply(world, bind, new HumanoidFullBodyIkTargets
{
    LeftHand = new Vector3(-0.4f, 1.2f, 0.3f),
    RightHand = new Vector3(0.4f, 1.2f, 0.3f),
});

// Persist IK aiming into local pose (clips / further FK)
HumanoidPoseSolver.BakeLocal(bind, world, pose);
```

## IK notes

- **Pole vectors** bend elbows/knees toward that side of the root→target line (`TwoBoneIk.EnforceBendSide` rejects inverted mids).
- **`BakeLocal`** writes parent-space quats from a world pose so animation systems can consume IK. Re-FK restores bind-length hierarchy (free mid offsets become aiming rotations).

## API

| Type | Role |
|------|------|
| `HumanoidPoseSolver` | FK `SolveWorld`; IK persist `BakeLocal` |
| `TwoBoneIk` | Arm / leg reach (`SolveMid`, `ApplyLimb`, `EnforceBendSide`) |
| `FabrikChain` | Generic N-link FABRIK on `Vector3` positions |
| `HumanoidChainIk` | FABRIK over a `HumanoidBone[]` chain into world pose |
| `HumanoidFullBodyIk` | Feet/hands via two-bone; optional head via spine chain |
| `HumanoidAnimationClip` | Clip schema (banks in `Novolis.Game.Humanoid`) |

## Bone standard

Mixamo / Unity Humanoid names (`HumanoidBone`, `HumanoidBoneNames.TryResolve`). Hierarchy under `HumanoidHierarchy`. Map to physics ragdoll spheres via `HumanoidRagdollMap` (indexes match `RagdollHumanoidPreset`).

## Not in this package

- Mesh skinning / GPU draw → `Humanoid.Skinning` / apps
- BVH / glTF import → `Humanoid.Import`
- Finger / face bones (later)
- Game-specific clip banks → `Novolis.Game.Humanoid`
- Planar agent walk → `Novolis.Simulation.Kinematics`

## Related packages

| Package | Role |
|---------|------|
| `Novolis.Simulation.Humanoid.Physics` | Ragdoll sphere bridge |
| `Novolis.Simulation.Humanoid.Import` | BVH / glTF joint import |
| `Novolis.Simulation.Humanoid.Skinning` | CPU LBS deform |
| `Novolis.Game.Humanoid` | Clip banks / body masks |
| `Novolis.Simulation.Kinematics` | Planar XZ agent motion (not IK) |
| `Novolis.Physics.Joints` | Ragdoll constraint dynamics |
