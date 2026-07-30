# Novolis.Simulation.View

Scene cameras, view poses, and controller rigs (orbit, free-look, tracking, first/third-person character).

## Install

```bash
dotnet add package Novolis.Simulation.View
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using System.Numerics;
using Novolis.Simulation.View;

var director = new CharacterCameraDirector();
var motor = new CharacterMotor(director.Look) { IsGrounded = true, GroundY = 0f };

motor.Tick(new MoveIntent(new Vector3(0, 0, 1), Jump: true), dt: 1f / 60f);
director.ApplyLook(new LookIntent(DeltaYaw: 0.1f, DeltaPitch: -0.05f));
director.SetMode(CharacterCameraMode.ThirdPerson);
var pose = director.Tick(1f / 60f);
```

Host apps map Raylib/Silk/Avalonia input into `LookIntent` / `MoveIntent`. Collision stays in World / apps.

## Related packages

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.Tiles` | PA-style build grids and pathfinding |
| `Novolis.Simulation.Voxels` | Chunked voxel worlds |
| `Novolis.Simulation` | Core simulation meta-package |
| `Novolis.Rendering.*` | GPU presentation (apps bridge Simulation → Rendering) |

## More documentation

- [Design](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/design.md)

## Support

Pre-release platform library. Public API is fully documented with strict XML (`CS1591` enforced).
