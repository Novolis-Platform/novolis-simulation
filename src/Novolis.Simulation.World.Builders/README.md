# Novolis.Simulation.World.Builders

Mesh builders for heightfields, occupancy columns, and enclosed rooms.

## Install

```bash
dotnet add package Novolis.Simulation.World.Builders
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.World.Builders;

var result = HeightfieldMeshBuilder.Build(sampler, extentMeters, resolution);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | Source heightfields and occupancy |
| `Novolis.Math.Geometry` | Mesh primitives |

## More documentation

- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
