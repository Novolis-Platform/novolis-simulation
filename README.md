# novolis-simulation

Composition layer for Novolis: **worlds** (occupancy grids), **view controllers**, and **kinematics** that combine `novolis-math` and `novolis-physics`.

## Packages

| Package | Role |
|---------|------|
| `Novolis.Simulation.World` | Planar occupancy on XZ (+Y up): move, LOS, raycast |
| `Novolis.Simulation.View` | `YawPitchController`, `ViewPose`, future RTS/CAD cameras |
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

See [simulation-layer-policy.md](../novolis-governance/docs/simulation-layer-policy.md).
