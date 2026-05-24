# Novolis.Simulation.Racing

Spline-based race tracks, car controllers, sensors, rewards, and ASCII race debugging.

## Install

```bash
dotnet add package Novolis.Simulation.Racing
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.Racing.Tracks;

var track = BuiltInTracks.Oval(rasterWidth: 128, rasterHeight: 96);
var simulation = new RaceSimulation(track);
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | Occupancy and heightfields |
| `Novolis.Simulation` | Core stack without racing |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
