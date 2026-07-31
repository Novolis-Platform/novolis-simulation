# Novolis.Simulation.Tiles

Prison Architect–style 2D build grids: layered tiles, edge walls/doors, room flood-fill, grid A*.

## Install

```bash
dotnet add package Novolis.Simulation.Tiles
```

## Quick start

```csharp
using Novolis.Simulation.Tiles;

var walls = new WallEdgeMap(16, 16);
walls.SetV(4, 3, WallEdge.Solid);
walls.SetH(2, 5, WallEdge.OpenDoor);

var rooms = RoomFloodFill.LabelRooms(walls);
var path = GridPathfinder.FindPath(walls, (0, 0), (15, 15));

var map = new TileMap2D(16, 16);
map.Set(TileLayerKind.Floor, 1, 1, 1);
var batch = new BuildBatch(16, 16);
batch.TouchCell(1, 1);
```

## API

| Type | Role |
|------|------|
| `TileMap2D` | Layered floor/object tiles |
| `WallEdgeMap` | H/V edge walls and doors; `BlocksStep` |
| `WallEdge` | `Solid`, `OpenDoor`, `None` presets |
| `RoomFloodFill` | `LabelRooms`, `CountRooms` |
| `GridPathfinder` | `FindPath` on wall grid |
| `WalkabilityMask` | Blocked cells from object layer |
| `BuildBatch` | Dirty rect tracking for incremental rebuilds |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | `PlanarOccupancy` for agent movement |
| `Novolis.Math.Arrays` | Compatible walkability grids |
