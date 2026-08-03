<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.Kinematics

Planar agent motion on XZ using grid occupancy or static-world sphere sweeps.

> **Not skeletal IK.** Bone FK / two-bone / FABRIK live in **`Novolis.Simulation.Humanoid`**. This package only resolves agent translation on the floor plane.

## Install

```bash
dotnet add package Novolis.Simulation.Kinematics
```

**Prerequisites:** [.NET 10 SDK](https://dotnet.microsoft.com/download) (`net10.0`).

## Quick start

```csharp
using Novolis.Simulation.Kinematics;

var next = PlanarAgent.Move(walls, position, delta, radius: 0.35f, cellSize: 1f, staticWorld);
```

## API

| Type | Role |
|------|------|
| `PlanarAgent` | `Move(walls, position, delta, radius, cellSize, staticWorld?, sweepCenterY?)` → resolved position |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation.World` | `DenseGrid` occupancy |
| `Novolis.Physics.Collision.Simple` | `IStaticWorld` BVH queries |
| `Novolis.Simulation.Humanoid` | Skeletal FK / IK / clips |

## More documentation

- [Getting started](https://github.com/Novolis-Platform/novolis-simulation/blob/main/docs/getting-started.md)
- [Library boundaries](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/library-boundaries.md)

