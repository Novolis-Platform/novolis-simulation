using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Physics.Abstractions;
using Novolis.Physics.Numerics;
using Novolis.Simulation.World;

namespace Novolis.Simulation.Kinematics;

/// <summary>Composed planar motion on XZ (+Y up): grid occupancy and/or BVH sweep.</summary>
public static class PlanarAgent
{
    /// <summary>Moves using grid collision when <paramref name="staticWorld"/> is null; otherwise BVH sweep.</summary>
    public static Vector2 Move(
        DenseGrid<byte> walls,
        Vector2 position,
        Vector2 delta,
        float radius,
        float cellSize,
        IStaticWorld? staticWorld = null,
        double sweepCenterY = 0.9)
    {
        if (delta.LengthSquared() < 1e-12f)
            return position;

        if (staticWorld is null)
            return PlanarOccupancy.TryMove(walls, position, delta, radius, cellSize);

        return SweepMove(staticWorld, position, delta, radius, sweepCenterY);
    }

    private static Vector2 SweepMove(
        IStaticWorld world,
        Vector2 position,
        Vector2 delta,
        double radius,
        double centerY)
    {
        var pos = new Vector3d(position.X, centerY, position.Y);
        var dx = new Vector3d(delta.X, 0, 0);
        if (System.Math.Abs(dx.X) > 1e-12)
        {
            var sphere = new Sphere3d(pos, radius);
            if (world.SweepSphere(sphere, dx, out var hit))
                pos = new Vector3d(pos.X + hit.Distance * System.Math.Sign(dx.X), pos.Y, pos.Z);
            else
                pos += dx;
        }

        var dz = new Vector3d(0, 0, delta.Y);
        if (System.Math.Abs(dz.Z) > 1e-12)
        {
            var sphere = new Sphere3d(pos, radius);
            if (world.SweepSphere(sphere, dz, out var hit))
                pos = new Vector3d(pos.X, pos.Y, pos.Z + hit.Distance * System.Math.Sign(dz.Z));
            else
                pos += dz;
        }

        return new Vector2((float)pos.X, (float)pos.Z);
    }
}

