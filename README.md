<!-- novolis-package-index:start -->
> **GitHub Packages shows this repository README on every package page** (upstream limitation).
> Open the **package README** for install and quick start — embedded in each .nupkg and linked below.

## Published packages

| Package | Install | Package README |
|---------|---------|----------------|
| `Novolis.Simulation` | `dotnet add package Novolis.Simulation` | [README](https://github.com/Novolis-Platform/novolis-simulation/blob/main/src/Novolis.Simulation/README.md) |
| `Novolis.Simulation.Abstractions` | `dotnet add package Novolis.Simulation.Abstractions` | [README](https://github.com/Novolis-Platform/novolis-simulation/blob/main/src/Novolis.Simulation.Abstractions/README.md) |
| `Novolis.Simulation.Kinematics` | `dotnet add package Novolis.Simulation.Kinematics` | [README](https://github.com/Novolis-Platform/novolis-simulation/blob/main/src/Novolis.Simulation.Kinematics/README.md) |
| `Novolis.Simulation.Racing` | `dotnet add package Novolis.Simulation.Racing` | [README](https://github.com/Novolis-Platform/novolis-simulation/blob/main/src/Novolis.Simulation.Racing/README.md) |
| `Novolis.Simulation.View` | `dotnet add package Novolis.Simulation.View` | [README](https://github.com/Novolis-Platform/novolis-simulation/blob/main/src/Novolis.Simulation.View/README.md) |
| `Novolis.Simulation.World` | `dotnet add package Novolis.Simulation.World` | [README](https://github.com/Novolis-Platform/novolis-simulation/blob/main/src/Novolis.Simulation.World/README.md) |
| `Novolis.Simulation.World.Builders` | `dotnet add package Novolis.Simulation.World.Builders` | [README](https://github.com/Novolis-Platform/novolis-simulation/blob/main/src/Novolis.Simulation.World.Builders/README.md) |

For NuGet.org and Visual Studio, the **embedded** README.md inside each package is authoritative.

<!-- novolis-package-index:end -->

# novolis-simulation

**Neutral simulation runtime** — not a game engine. Worlds, systems, time, observers, and kinematics that compose Math, Geometry, and Physics.

## Packages

| Package | Role |
|---------|------|
| `Novolis.Simulation.World` | Planar occupancy on XZ (+Y up): move, LOS, raycast |
| `Novolis.Simulation.View` | All platform cameras: `YawPitchController`, third-person, orbit, `ViewPose`, … |
| `Novolis.Simulation.Kinematics` | `PlanarAgent.Move` (grid and/or BVH) |
| `Novolis.Simulation.World.Builders` | `OccupancyColumnMeshBuilder` → `BvhStaticWorld` |
| `Novolis.Simulation` | Meta package referencing all of the above |

## Build

```powershell
cd d:\novolis\novolis-math
.\scripts\pack-local.ps1
cd d:\novolis\novolis-physics
.\scripts\pack-local.ps1
cd d:\novolis\novolis-simulation
dotnet build Novolis.Simulation.slnx
.\scripts\pack-local.ps1
```

## Policy

See [library-boundaries.md](../novolis-governance/docs/library-boundaries.md).

