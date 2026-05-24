# Novolis.Simulation.Kinematics

Planar agent motion on XZ using grid occupancy or static-world sphere sweeps.

## Install

```bash
dotnet add package Novolis.Simulation.Kinematics
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.Kinematics;

var next = PlanarAgent.Move(walls, position, delta, radius: 0.35f, cellSize: 1f, staticWorld);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | `DenseGrid` occupancy |
| `Novolis.Physics.Collision.Simple` | `IStaticWorld` BVH queries |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
