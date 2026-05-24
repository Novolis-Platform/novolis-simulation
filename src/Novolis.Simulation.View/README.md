# Novolis.Simulation.View

Scene cameras, view poses, and controller rigs (orbit, free-look, tracking).

## Install

```bash
dotnet add package Novolis.Simulation.View
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.View;

var camera = new Camera { Position = new(0, 5, 10), Target = Vector3.Zero };
var rig = new OrbitCameraRig { Distance = 12f };
```

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation` | Core simulation meta-package |
| `Novolis.Rendering.*` | GPU presentation (apps bridge Simulation → Rendering) |

## More documentation

- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
