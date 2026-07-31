# Novolis.Simulation.Racing

Spline-based race tracks, car controllers, sensors, rewards, and ASCII race debugging.

## Install

```bash
dotnet add package Novolis.Simulation.Racing
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Race;
using Novolis.Simulation.Racing.Tracks;

var track = new TrackBuilder().Build(BuiltInTracks.Oval);
var simulation = new RaceSimulation(track, [new FullThrottleController()]);
simulation.Reset();
simulation.Tick();
```

## API

| Type | Role |
|------|------|
| `RaceSimulation` | `Track`, `Controllers`, `State`, `Reset`, `Tick` |
| `RaceTrack` | Spline loop, gates, progress map |
| `BuiltInTracks` | `Circle`, `Oval`, `Stadium`, `Chicane`, …; `All`, `ById` |
| `TrackBuilder` | `Build(ITrackDefinition)` |
| `DefaultCarSensorModel` | Raycast sensor readings |
| `DefaultRewardModel` | Lap/progress reward breakdown |
| `LapScorer` | Lap timing and standings |
| `RaceAsciiRenderer` | Terminal debug view |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | Occupancy and heightfields |
| `Novolis.Simulation` | Core stack without racing |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)
