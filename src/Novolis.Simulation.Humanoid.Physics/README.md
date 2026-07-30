# Novolis.Simulation.Humanoid.Physics

Adapter between `Novolis.Simulation.Humanoid` and `Novolis.Physics.Joints.RagdollHumanoidPreset`.

## Install

```bash
dotnet add package Novolis.Simulation.Humanoid.Physics
```

## Quick start

```csharp
var bind = HumanoidBindPose.CreateDefaultTPose();
var spheres = new List<SphereState>();
var joints = new List<DistanceJoint>();
var swings = new List<SwingLimit>();
var hinges = new List<HingeLimit>();

HumanoidRagdollBridge.BuildStandingFromBind(bind, spheres, joints, swings, hinges);

var world = HumanoidPoseSolver.SolveWorld(bind, HumanoidPose.FromBind(bind));
HumanoidRagdollBridge.ApplyWorldPoseToSpheres(world, spheres);
```
