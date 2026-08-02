# Getting started

**novolis-simulation** composes worlds, cameras, planar agents, and optional racing scenarios on top of **novolis-physics**.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)

## Install

Meta-package (core stack without racing):

```bash
dotnet add package Novolis.Simulation
```

Add racing separately when needed:

```bash
dotnet add package Novolis.Simulation.Racing
```

## Quick start

```csharp
using Novolis.Math.Arrays;
using Novolis.Simulation.World;

var world = new SimulationWorld(new DenseGrid<byte>(width, height), cellSize: 1f);
```

## Humanoid IK (skeletal)

Planar agent move is `Novolis.Simulation.Kinematics`. Skeletal FK/IK is `Novolis.Simulation.Humanoid`:

```csharp
using Novolis.Simulation.Humanoid;

var bind = HumanoidBindPose.CreateDefaultTPose(1.8f);
var pose = HumanoidPose.FromBind(bind);
var worldPose = HumanoidPoseSolver.SolveWorld(bind, pose);
var targets = HumanoidFullBodyIkTargets.WithDefaults();
targets.LeftHand = worldPose.Position(HumanoidBone.LeftHand) + new System.Numerics.Vector3(-0.1f, 0.2f, 0.1f);
HumanoidFullBodyIk.Apply(worldPose, bind, targets);
HumanoidPoseSolver.BakeLocal(bind, worldPose, pose);
```

Dogfood: `d:\novolis\novolis-dogfooding\apps\avalonia\HumanoidLab` (`--smoke` for headless).

## More documentation

- [Design](design.md)
- [Release](release.md)
