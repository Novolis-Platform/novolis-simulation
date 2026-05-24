# Novolis.Simulation (meta-package)

Installs the core simulation stack (world, view, kinematics, builders, abstractions) in one reference.

## Install

```bash
dotnet add package Novolis.Simulation
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Math.Arrays;
using Novolis.Simulation.World;

var world = new SimulationWorld(new DenseGrid<byte>(64, 64), cellSize: 1f);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Racing` | Tracks, race loop, car AI |
| `Novolis.Physics` | Underlying force-first physics |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
