<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

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

## API

| Type | Role |
|------|------|
| `MeshState` | Nodes, edges, packets, mailboxes |
| `MeshEngine` | Composable `IMeshStep` pipeline |
| `DefaultMeshPipeline` | Stock publish/flood/TTL/mailbox steps |
| `MeshPathfinder` | `FindPath(state, origin, destination)` |
| `PublishEngine`, `FloodEngine`, `TtlEngine` | Traffic layer processors |
| `MailboxEngine`, `FeedEngine` | Store-and-forward delivery |
| `MeshTestGraph` | Sample topologies for tests |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | Spatial world / occupancy |
| `Novolis.Simulation` | Core simulation meta-package |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)

