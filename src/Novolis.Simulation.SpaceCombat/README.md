# Novolis.Simulation.SpaceCombat

Headless arcade space combat: craft profiles, intent-driven flight, laser bolts, targeting, and dual-role mission phases (`Freighter` → `Transfer` → `Fighter`).

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
session.Tick(new FlightIntent { ThrottleUp = 1f, Fire = true }, 1f / 60f);
```

## API

| Type | Role |
|------|------|
| `MissionSession` | Dual-role phases, craft state, bolts, targets |
| `MissionDescriptor` | Profiles, protect timer, kill goals |
| `CraftProfile` | Speed, turn, hull, hit radius |
| `FlightIntent` | Throttle / pitch / yaw / fire / transfer |
| `LaserBolt` | Active bolt state |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.View` | `CraftCamera` cockpit / chase-aft poses |
| `Novolis.Simulation` | Core simulation stack |
