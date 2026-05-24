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

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Kinematics` | BVH sweeps via `PlanarAgent` |
| `Novolis.Simulation.World.Builders` | Generate meshes from worlds |

## More documentation

- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
