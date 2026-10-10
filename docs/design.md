# Design

Simulation sits above **novolis-physics** and **novolis-math**: it owns world grids, cameras, and game-facing composition, not low-level integrators.

## Package layout

| Package | Role |
|---------|------|
| `Novolis.Simulation.Abstractions` | `ISimulationObject`, `ISimulationSystem`, `SimulationStep` |
| `Novolis.Simulation.World` | Occupancy grids, heightfields, room bounds |
| `Novolis.Simulation.View` | Cameras and rigs (`ViewPose`) for apps/rendering bridges |
| `Novolis.Simulation.Kinematics` | Planar agent motion against grids or BVH |
| `Novolis.Simulation.World.Builders` | Mesh builders for heightfields and rooms |
| `Novolis.Simulation.Humanoid` | Mixamo/Unity biped: T-pose, FK/IK, clip schema |
| `Novolis.Simulation.Humanoid.Physics` | Ragdoll sphere bridge to `Physics.Joints` |
| `Novolis.Simulation.Humanoid.Import` | BVH mocap + lightweight glTF joint import |
| `Novolis.Simulation.Humanoid.Skinning` | CPU linear-blend skinning over `TriangleMesh` |
| `Novolis.Simulation.Tiles` | PA-style layers, edge walls/doors, room flood-fill, grid A* |
| `Novolis.Simulation.Voxels` | Chunked block world, streamer, dig/place, terrain fill |
| `Novolis.Simulation.Voxels.Meshing` | Face-culled / greedy mesh → `Math.Geometry` (no GPU) |
| `Novolis.Simulation.Replay` | Recording and replay |
| `Novolis.Simulation.Racing` | Track definitions, race loop, sensors (headless) |
| `Novolis.Simulation.SpaceCombat` | Headless arcade flight, bolts, missions |
| `Novolis.Simulation.Mesh` | DTN / relay packet mesh — not triangle geometry |
| `Novolis.Simulation` | Meta-package referencing the core stack |

## Coordinates

Right-handed 3D with **+Y up**; planar gameplay uses **XZ** with `Y = 0` per platform stack policy.
