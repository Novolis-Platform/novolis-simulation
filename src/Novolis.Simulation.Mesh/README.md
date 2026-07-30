# Novolis.Simulation.Mesh

DTN/relay mesh kernel: publish, flood, TTL, mailbox, feeds, and pathfinding for delayed packet traffic.

## Install

```bash
dotnet add package Novolis.Simulation.Mesh
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.Mesh;

var state = MeshTestGraph.Triangle();
var path = MeshPathfinder.FindPath(state, MeshTestGraph.Sol, MeshTestGraph.Wolf);
state = DefaultMeshPipeline.Advance(state);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | Spatial world / occupancy |
| `Novolis.Simulation` | Core simulation meta-package |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
