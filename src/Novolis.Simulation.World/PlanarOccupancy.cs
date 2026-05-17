using System.Numerics;
using Novolis.Math.Arrays;

namespace Novolis.Simulation.World;

/// <summary>
/// Circle-vs-grid collision on the XZ plane. Cell value <c>0</c> is walkable; any other value blocks.
/// </summary>
public static class PlanarOccupancy
{
    /// <summary>
    /// Moves <paramref name="position"/> by <paramref name="delta"/> (XZ), sliding along walls.
    /// </summary>
    public static Vector3 TryMove(
        DenseGrid<byte> map,
        Vector3 position,
        Vector3 delta,
        float radius,
        float cellSize = 1f)
    {
        if (delta.LengthSquared() < 1e-12f)
            return position;

        var next = position + new Vector3(delta.X, 0f, 0f);
        if (!OverlapsWall(map, next, radius, cellSize))
            position = next;

        next = position + new Vector3(0f, 0f, delta.Z);
        if (!OverlapsWall(map, next, radius, cellSize))
            position = next;

        return position;
    }

    /// <summary>Returns true when the circle at <paramref name="position"/> overlaps a blocked cell.</summary>
    public static bool OverlapsWall(DenseGrid<byte> map, Vector3 position, float radius, float cellSize = 1f)
    {
        var minX = (int)MathF.Floor((position.X - radius) / cellSize);
        var maxX = (int)MathF.Floor((position.X + radius) / cellSize);
        var minZ = (int)MathF.Floor((position.Z - radius) / cellSize);
        var maxZ = (int)MathF.Floor((position.Z + radius) / cellSize);

        for (var z = minZ; z <= maxZ; z++)
        for (var x = minX; x <= maxX; x++)
        {
            if (x < 0 || z < 0 || x >= map.Width || z >= map.Height)
                return true;

            if (!IsBlocked(map, (uint)x, (uint)z))
                continue;

            var cellMinX = x * cellSize;
            var cellMinZ = z * cellSize;
            var cellMaxX = cellMinX + cellSize;
            var cellMaxZ = cellMinZ + cellSize;

            var closestX = System.Math.Clamp(position.X, cellMinX, cellMaxX);
            var closestZ = System.Math.Clamp(position.Z, cellMinZ, cellMaxZ);
            var dx = position.X - closestX;
            var dz = position.Z - closestZ;
            if (dx * dx + dz * dz < radius * radius)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves circle overlap with blocked cells by pushing <paramref name="position"/> toward open floor.
    /// </summary>
    public static Vector3 PushOutOfWalls(
        DenseGrid<byte> map,
        Vector3 position,
        float radius,
        float cellSize = 1f,
        int maxIterations = 8)
    {
        var pos = position;
        for (var iter = 0; iter < maxIterations; iter++)
        {
            if (!OverlapsWall(map, pos, radius, cellSize))
                return pos;

            var push = Vector3.Zero;
            var minX = (int)MathF.Floor((pos.X - radius) / cellSize);
            var maxX = (int)MathF.Floor((pos.X + radius) / cellSize);
            var minZ = (int)MathF.Floor((pos.Z - radius) / cellSize);
            var maxZ = (int)MathF.Floor((pos.Z + radius) / cellSize);

            for (var z = minZ; z <= maxZ; z++)
            for (var x = minX; x <= maxX; x++)
            {
                if (!IsCellBlocked(map, x, z))
                    continue;

                var cellMinX = x * cellSize;
                var cellMinZ = z * cellSize;
                var cellMaxX = cellMinX + cellSize;
                var cellMaxZ = cellMinZ + cellSize;

                var closestX = System.Math.Clamp(pos.X, cellMinX, cellMaxX);
                var closestZ = System.Math.Clamp(pos.Z, cellMinZ, cellMaxZ);
                var dx = pos.X - closestX;
                var dz = pos.Z - closestZ;
                var distSq = dx * dx + dz * dz;
                float pushDistance;
                if (distSq < 1e-10f)
                {
                    if (!TryGetEscapeDirection(map, x, z, cellSize, out dx, out dz))
                    {
                        dx = 1f;
                        dz = 0f;
                    }

                    pushDistance = radius + cellSize * 0.5f;
                }
                else
                {
                    var dist = MathF.Sqrt(distSq);
                    pushDistance = radius - dist + 1e-3f;
                    if (pushDistance <= 0f)
                        continue;

                    dx /= dist;
                    dz /= dist;
                }

                push += new Vector3(dx, 0f, dz) * pushDistance;
            }

            if (push.LengthSquared() < 1e-10f)
                break;

            pos += push;
        }

        return pos;
    }

    /// <summary>
    /// Tests whether a look ray hits a columnar agent on the XZ plane (ignores pitch/Y).
    /// </summary>
    public static bool TryRaycastDisc(
        Vector3 rayOrigin,
        Vector3 rayDirection,
        Vector3 targetPosition,
        float maxRange,
        float hitRadius,
        out PlanarDiscHit hit)
    {
        hit = default;
        var ox = rayOrigin.X;
        var oz = rayOrigin.Z;
        var dx = rayDirection.X;
        var dz = rayDirection.Z;
        var lenSq = dx * dx + dz * dz;
        if (lenSq < 1e-8f)
            return false;

        var invLen = 1f / MathF.Sqrt(lenSq);
        dx *= invLen;
        dz *= invLen;

        var ex = targetPosition.X;
        var ez = targetPosition.Z;
        var t = (ex - ox) * dx + (ez - oz) * dz;
        if (t < 0.25f || t > maxRange)
            return false;

        var closestX = ox + dx * t;
        var closestZ = oz + dz * t;
        var missX = closestX - ex;
        var missZ = closestZ - ez;
        var missSq = missX * missX + missZ * missZ;
        if (missSq > hitRadius * hitRadius)
            return false;

        hit = new PlanarDiscHit { DistanceAlongRay = t };
        return true;
    }

    /// <summary>
    /// Casts a ray on the XZ plane and returns true when a blocked cell is hit before <paramref name="maxDistance"/>.
    /// </summary>
    public static bool TryRaycastWall(
        DenseGrid<byte> map,
        Vector3 origin,
        Vector3 direction,
        float maxDistance,
        float cellSize,
        out float hitDistance)
    {
        hitDistance = 0f;

        var dx = direction.X;
        var dz = direction.Z;
        var lenSq = dx * dx + dz * dz;
        if (lenSq < 1e-10f)
            return false;

        var invLen = 1f / MathF.Sqrt(lenSq);
        dx *= invLen;
        dz *= invLen;

        var startCellX = (int)MathF.Floor(origin.X / cellSize);
        var startCellZ = (int)MathF.Floor(origin.Z / cellSize);
        var step = cellSize * 0.25f;
        var dist = step;

        while (dist <= maxDistance)
        {
            var px = origin.X + dx * dist;
            var pz = origin.Z + dz * dist;
            var cx = (int)MathF.Floor(px / cellSize);
            var cz = (int)MathF.Floor(pz / cellSize);
            if ((cx != startCellX || cz != startCellZ) && IsCellBlocked(map, cx, cz))
            {
                hitDistance = dist;
                return true;
            }

            dist += step;
        }

        return false;
    }

    /// <summary>
    /// Returns true when no blocked cells lie on the grid line from <paramref name="from"/> to <paramref name="to"/> (XZ world space).
    /// Uses grid DDA (supercover-style traversal). The start cell is ignored.
    /// When <paramref name="clearanceRadius"/> is positive, blocked cells near the segment also block sight.
    /// </summary>
    public static bool HasLineOfSight(
        DenseGrid<byte> map,
        Vector3 from,
        Vector3 to,
        float cellSize = 1f,
        float clearanceRadius = 0f)
    {
        var startX = from.X / cellSize;
        var startZ = from.Z / cellSize;
        var endX = to.X / cellSize;
        var endZ = to.Z / cellSize;

        var cellX = (int)MathF.Floor(startX);
        var cellZ = (int)MathF.Floor(startZ);
        var endCellX = (int)MathF.Floor(endX);
        var endCellZ = (int)MathF.Floor(endZ);

        var stepX = startX < endX ? 1 : startX > endX ? -1 : 0;
        var stepZ = startZ < endZ ? 1 : startZ > endZ ? -1 : 0;

        var tDeltaX = stepX == 0 ? float.MaxValue : MathF.Abs(1f / (endX - startX));
        var tDeltaZ = stepZ == 0 ? float.MaxValue : MathF.Abs(1f / (endZ - startZ));

        var fracX = startX - MathF.Floor(startX);
        var fracZ = startZ - MathF.Floor(startZ);
        var tMaxX = stepX > 0
            ? (1f - fracX) * tDeltaX
            : stepX < 0
                ? fracX * tDeltaX
                : float.MaxValue;
        var tMaxZ = stepZ > 0
            ? (1f - fracZ) * tDeltaZ
            : stepZ < 0
                ? fracZ * tDeltaZ
                : float.MaxValue;

        var startCellX = cellX;
        var startCellZ = cellZ;

        while (true)
        {
            if (cellX != startCellX || cellZ != startCellZ)
            {
                if (IsCellBlocked(map, cellX, cellZ))
                    return false;

                if (clearanceRadius > 0f
                    && ClearanceBlocked(map, from, to, cellX, cellZ, cellSize, clearanceRadius))
                    return false;
            }

            if (cellX == endCellX && cellZ == endCellZ)
                return true;

            if (tMaxX < tMaxZ)
            {
                cellX += stepX;
                tMaxX += tDeltaX;
            }
            else
            {
                cellZ += stepZ;
                tMaxZ += tDeltaZ;
            }
        }
    }

    private static bool ClearanceBlocked(
        DenseGrid<byte> map,
        Vector3 from,
        Vector3 to,
        int pathCellX,
        int pathCellZ,
        float cellSize,
        float clearanceRadius)
    {
        var radiusCells = (int)MathF.Ceiling(clearanceRadius / cellSize);
        for (var dz = -radiusCells; dz <= radiusCells; dz++)
        for (var dx = -radiusCells; dx <= radiusCells; dx++)
        {
            var x = pathCellX + dx;
            var z = pathCellZ + dz;
            if (x == pathCellX && z == pathCellZ)
                continue;
            if (!IsCellBlocked(map, x, z))
                continue;

            var center = new Vector3((x + 0.5f) * cellSize, 0f, (z + 0.5f) * cellSize);
            if (DistancePointToSegment(center, from, to) <= clearanceRadius + cellSize * 0.5f)
                return true;
        }

        return false;
    }

    private static float DistancePointToSegment(Vector3 point, Vector3 a, Vector3 b)
    {
        var ab = b - a;
        var lenSq = ab.LengthSquared();
        if (lenSq < 1e-12f)
            return Vector3.Distance(point, a);

        var t = System.Math.Clamp(Vector3.Dot(point - a, ab) / lenSq, 0f, 1f);
        var closest = a + ab * t;
        return Vector3.Distance(point, closest);
    }

    private static bool TryGetEscapeDirection(
        DenseGrid<byte> map,
        int wallX,
        int wallZ,
        float cellSize,
        out float dx,
        out float dz)
    {
        dx = 0f;
        dz = 0f;
        var bestDistSq = float.MaxValue;
        var found = false;

        foreach (var (ox, oz) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
        {
            var nx = wallX + ox;
            var nz = wallZ + oz;
            if (IsCellBlocked(map, nx, nz))
                continue;

            var openX = (nx + 0.5f) * cellSize;
            var openZ = (nz + 0.5f) * cellSize;
            var centerX = (wallX + 0.5f) * cellSize;
            var centerZ = (wallZ + 0.5f) * cellSize;
            var ex = openX - centerX;
            var ez = openZ - centerZ;
            var distSq = ex * ex + ez * ez;
            if (distSq >= bestDistSq)
                continue;

            bestDistSq = distSq;
            dx = ex;
            dz = ez;
            found = true;
        }

        return found;
    }

    private static bool IsCellBlocked(DenseGrid<byte> map, int x, int z)
    {
        if (x < 0 || z < 0 || x >= map.Width || z >= map.Height)
            return true;

        return IsBlocked(map, (uint)x, (uint)z);
    }

    private static bool IsBlocked(DenseGrid<byte> map, uint x, uint z)
    {
        return map.Get(x, z) != 0;
    }
}
