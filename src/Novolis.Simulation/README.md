<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation

Installs the core simulation stack (world, view, kinematics, builders, abstractions) in one reference.

## Install

```bash
dotnet add package Novolis.Simulation
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Math.Arrays;
using Novolis.Simulation;
using Novolis.Simulation.World;

var world = new SimulationWorld(new DenseGrid<byte>(64, 64), cellSize: 1f);
var clock = new SimulationClock(fixedDeltaSeconds: 1.0 / 60);
var step = clock.Advance();
```

## API

| Type | Role |
|------|------|
| `SimulationClock` | `Advance()` → `SimulationStep`; `FixedDeltaSeconds`, `ElapsedSeconds`, `Tick`, `Reset` |

Bundled transitively: Abstractions, World, View, Kinematics, World.Builders.

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Racing` | Tracks, race loop, car AI |
| `Novolis.Simulation.Replay` | Deterministic tick replay |
| `Novolis.Physics` | Underlying force-first physics |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)
- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)

