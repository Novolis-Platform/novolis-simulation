<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

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

## API

| Type | Role |
|------|------|
| `SimulationStep` | `DeltaSeconds`, `Tick` |
| `ISimulationObject` | `Guid Id` |
| `ISimulationState` | `IReadOnlyList<ISimulationObject> Objects` |
| `ISimulationSystem` | `Step(state, step)` |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation` | `SimulationClock` and meta-package |
| `Novolis.Simulation.Replay` | Tick timelines and verification |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)

