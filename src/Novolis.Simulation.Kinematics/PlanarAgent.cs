using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Math.Geometry;
using Novolis.Physics.Abstractions;
using Novolis.Simulation.World;

namespace Novolis.Simulation.Kinematics;

/// <summary>Composed planar motion on XZ (+Y up): grid occupancy and/or BVH sweep.</summary>
public static class PlanarAgent
{
    /// <summary>Moves using grid collision when <paramref name="staticWorld"/> is null; otherwise BVH sweep.</summary>
    public static Vector3 Move(
        DenseGrid<byte> walls,
        Vector3 position,
        Vector3 delta,
        float radius,
        float cellSize,
        IStaticWorld? staticWorld = null,
        float sweepCenterY = 0.9f)
    {
        if (delta.LengthSquared() < 1e-12f)
            return position;

        if (staticWorld is null)
            return PlanarOccupancy.TryMove(walls, position, delta, radius, cellSize);

        return SweepMove(staticWorld, position, delta, radius, sweepCenterY);
    }

    private static Vector3 SweepMove(
        IStaticWorld world,
        Vector3 position,
        Vector3 delta,
        float radius,
        float centerY)
    {
        var pos = new Vector3(position.X, centerY, position.Z);
        var dx = new Vector3(delta.X, 0f, 0f);
        if (MathF.Abs(dx.X) > 1e-6f)
        {
            var sphere = new Sphere3(pos, radius);
            if (world.SweepSphere(in sphere, dx, out var hit))
                pos = new Vector3(pos.X + (float)hit.Distance * MathF.Sign(dx.X), pos.Y, pos.Z);
            else
                pos += dx;
        }

        var dz = new Vector3(0f, 0f, delta.Z);
        if (MathF.Abs(dz.Z) > 1e-6f)
        {
            var sphere = new Sphere3(pos, radius);
            if (world.SweepSphere(in sphere, dz, out var hit))
                pos = new Vector3(pos.X, pos.Y, pos.Z + (float)hit.Distance * MathF.Sign(dz.Z));
            else
                pos += dz;
        }

        return new Vector3(pos.X, 0f, pos.Z);
    }
}
