<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.World

Occupancy grids, bounded heightfields, room interiors, and planar collision helpers.

## Install

```bash
dotnet add package Novolis.Simulation.World
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.World;

var world = new SimulationWorld(occupancyGrid, cellSize: 1f);
var moved = PlanarOccupancy.TryMove(world.Occupancy, position, delta, radius: 0.4f, world.CellSize);
```

## API

| Type | Role |
|------|------|
| `SimulationWorld` | Wraps `DenseGrid<byte>` occupancy + cell size |
| `BoundedHeightfield` | Height sampling, projectile contact, surface projection |
| `PlanarOccupancy` | `TryMove`, `OverlapsWall`, `PushOutOfWalls`, raycasts, line-of-sight |
| `RoomInteriorBounds` | Enclosed room clamp volume |
| `WorldExtentOptions` | Extent meters and collision/draw resolution |
| `PlanarDiscHit` | Raycast hit distance along ray |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Kinematics` | BVH sweeps via `PlanarAgent` |
| `Novolis.Simulation.World.Builders` | Generate meshes from worlds |

## More documentation

- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)

