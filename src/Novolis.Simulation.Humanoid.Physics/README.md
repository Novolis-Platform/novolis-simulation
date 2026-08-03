<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.Humanoid.Physics

Adapter between `Novolis.Simulation.Humanoid` and `Novolis.Physics.Joints.RagdollHumanoidPreset`.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Physics
```

## Quick start

```csharp
using Novolis.Physics.Collision.Simple;
using Novolis.Physics.Joints;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Humanoid.Physics;

var bind = HumanoidBindPose.CreateDefaultTPose();
var spheres = new List<SphereState>();
var joints = new List<DistanceJoint>();
var swings = new List<SwingLimit>();
var hinges = new List<HingeLimit>();

HumanoidRagdollBridge.BuildStandingFromBind(bind, spheres, joints, swings, hinges);

var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
HumanoidRagdollBridge.ApplyWorldPoseToSpheres(world, spheres);
```

## API

| Type | Role |
|------|------|
| `HumanoidRagdollBridge` | `SphereCount`; `BuildStandingFromBind`; `ApplyBindToSpheres`; `ApplyWorldPoseToSpheres`; `WorldPoseFromSpheres` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Humanoid` | Poses, bind frames, bone schema |
| `Novolis.Physics.Joints` | `RagdollHumanoidPreset`, joint solvers |
| `Novolis.Simulation.Humanoid.Skinning` | Deform mesh to ragdoll spheres |

