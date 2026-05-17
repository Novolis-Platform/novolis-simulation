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
