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

Depends on `Novolis.Math.Arrays` only (walkability grids are compatible with `Novolis.Simulation.World.PlanarOccupancy`).
