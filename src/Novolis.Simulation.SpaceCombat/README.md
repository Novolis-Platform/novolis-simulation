<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.SpaceCombat

Headless arcade space combat: craft profiles, intent-driven flight, laser bolts, targeting, dual-role mission phases (`Freighter` → `Transfer` → `Fighter`), and crew stations with heuristic (optionally neural-host) pilot/gunner AI.

No Raylib dependency. Pair with `Novolis.Simulation.View.CraftCamera` for cockpit/chase poses.

## Install

```bash
dotnet add package Novolis.Simulation.SpaceCombat
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.SpaceCombat;

var session = new MissionSession(new MissionDescriptor
{
    Id = "escort",
    FreighterProfile = CraftProfile.FreighterDefault,
    FighterProfile = CraftProfile.FighterDefault,
    HostileProfile = CraftProfile.HostileDefault,
    HostileCount = 3,
    ProtectSeconds = 20f,
    DestroyRequired = 3,
});
session.Begin();
session.CrewStation = CrewStation.Gunner; // AI pilots; human aims/fires
session.Tick(new FlightIntent { Fire = true }, 1f / 60f);
```

## API

| Type | Role |
|------|------|
| `MissionSession` | Dual-role phases, craft state, bolts, targets, crew AI |
| `CrewStation` | `Dual` / `Pilot` (AI gunner) / `Gunner` (AI pilot) |
| `HeuristicPilotAi` / `HeuristicGunnerAi` | Default crew controllers |
| `MissionDescriptor` | Profiles, protect timer, kill goals |
| `CraftProfile` | Speed, turn, hull, hit radius |
| `FlightIntent` | Throttle / pitch / yaw / fire / transfer |
| `LaserBolt` | Active bolt state |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.View` | `CraftCamera` cockpit / chase-aft poses |
| `Novolis.MachineLearning.Neural` | `ContinuousActionPolicy` for neural crew imitation |
| `Novolis.Simulation` | Core simulation stack |

