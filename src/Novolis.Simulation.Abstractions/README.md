# Novolis.Simulation.Abstractions

Core simulation contracts: objects, shared state, systems, and fixed-step ticks.

## Install

```bash
dotnet add package Novolis.Simulation.Abstractions
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.Abstractions;

public sealed class MySystem : ISimulationSystem
{
    public void Step(ISimulationState state, in SimulationStep step) { /* ... */ }
}
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | Concrete world state |
| `Novolis.Simulation` | Meta-package for the core stack |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
