# Novolis.Simulation.World.Builders

Mesh builders for heightfields, occupancy columns, and enclosed rooms.

## Install

```bash
dotnet add package Novolis.Simulation.World.Builders
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.World;
using Novolis.Simulation.World.Builders;

var options = new WorldExtentOptions { ExtentMeters = 64, CollisionCells = 64, DrawCells = 32 };
var result = HeightfieldMeshBuilder.Build(sampler, options);
var collision = result.Collision; // BvhStaticWorld
```

## API

| Type | Role |
|------|------|
| `HeightfieldMeshBuilder` | `Build(sampler, options)` → collision + draw mesh |
| `HeightfieldBuildResult` | `Collision`, `DrawVertices`, `DrawCells` |
| `OccupancyColumnMeshBuilder` | Wall columns from occupancy grid |
| `OccupancyEnclosedRoomMeshBuilder` | Enclosed room shell from walls |
| `RoomMeshBuilder` | `AppendBox`, `AppendQuad` helpers |
| `InteriorClampVolumeExtensions` | `ToInteriorClamp(RoomInteriorBounds)` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | Source heightfields and occupancy |
| `Novolis.Math.Geometry` | Mesh primitives |

## More documentation

- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)
