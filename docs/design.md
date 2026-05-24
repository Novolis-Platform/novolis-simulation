# Design

Simulation sits above **novolis-physics** and **novolis-math**: it owns world grids, cameras, and game-facing composition, not low-level integrators.

## Package layout

| Package | Role |
|---------|------|
| `Novolis.Simulation.Abstractions` | `ISimulationObject`, `ISimulationSystem`, `SimulationStep` |
| `Novolis.Simulation.World` | Occupancy grids, heightfields, room bounds |
| `Novolis.Simulation.View` | Cameras and rigs for apps/rendering bridges |
| `Novolis.Simulation.Kinematics` | Planar agent motion against grids or BVH |
| `Novolis.Simulation.World.Builders` | Mesh builders for heightfields and rooms |
| `Novolis.Simulation.Racing` | Track definitions, race loop, sensors (optional) |
| `Novolis.Simulation` | Meta-package referencing the core stack |

## Coordinates

Right-handed 3D with **+Y up**; planar gameplay uses **XZ** with `Y = 0` per platform stack policy.
