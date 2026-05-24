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

## More documentation

- [Design](design.md)
- [Release](release.md)
