using System.Numerics;
using Novolis.Physics.Collision.Simple;

namespace Novolis.Simulation.World.Builders;

/// <summary>Builds an enclosed <see cref="BvhStaticWorld"/> from an occupancy grid (walls + optional floor and ceiling).</summary>
public static class OccupancyEnclosedRoomMeshBuilder
{
    public static BvhStaticWorld FromWallGrid(
        uint width,
        uint height,
        ReadOnlySpan<byte> cells,
        float cellSize = 1f,
        float wallHeight = 2f,
        bool includeFloor = true,
        bool includeCeiling = true)
    {
        var verts = new List<Vector3>();
        var tris = new List<int>();

        for (var y = 0u; y < height; y++)
        for (var x = 0u; x < width; x++)
        {
            var index = (int)(y * width + x);
            if (index >= cells.Length || cells[index] == 0)
                continue;

            var cx = (x + 0.5f) * cellSize;
            var cz = (y + 0.5f) * cellSize;
            var h = wallHeight * 0.5f;
            var hx = cellSize * 0.5f;
            RoomMeshBuilder.AppendBox(verts, tris, cx, h, cz, hx, h, hx);
        }

        if (includeFloor || includeCeiling)
        {
            var x0 = cellSize;
            var x1 = (width - 1) * cellSize;
            var z0 = cellSize;
            var z1 = (height - 1) * cellSize;

            if (includeFloor)
            {
                RoomMeshBuilder.AppendQuad(
                    verts,
                    tris,
                    new(x0, 0f, z0),
                    new(x1, 0f, z0),
                    new(x1, 0f, z1),
                    new(x0, 0f, z1));
            }

            if (includeCeiling)
            {
                RoomMeshBuilder.AppendQuad(
                    verts,
                    tris,
                    new(x0, wallHeight, z1),
                    new(x1, wallHeight, z1),
                    new(x1, wallHeight, z0),
                    new(x0, wallHeight, z0));
            }
        }

        return new BvhStaticWorld(new StaticTriangleMesh(verts.ToArray(), tris.ToArray()));
    }
}
