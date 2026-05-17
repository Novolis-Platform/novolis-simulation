using System.Numerics;
using Novolis.Physics.Collision.Simple;

namespace Novolis.Simulation.World.Builders;

/// <summary>Builds a <see cref="BvhStaticWorld"/> from an occupancy grid (XZ plane, +Y up).</summary>
public static class OccupancyColumnMeshBuilder
{
    public static BvhStaticWorld FromWallGrid(
        uint width,
        uint height,
        ReadOnlySpan<byte> cells,
        float cellSize = 1f,
        float wallHeight = 2f)
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
            AddBox(verts, tris, cx, h, cz, hx, h, hx);
        }

        return new BvhStaticWorld(new StaticTriangleMesh(verts.ToArray(), tris.ToArray()));
    }

    private static void AddBox(
        List<Vector3> verts,
        List<int> tris,
        float cx,
        float cy,
        float cz,
        float hx,
        float hy,
        float hz)
    {
        var x0 = cx - hx;
        var x1 = cx + hx;
        var y0 = cy - hy;
        var y1 = cy + hy;
        var z0 = cz - hz;
        var z1 = cz + hz;

        AddQuad(verts, tris, new(x0, y0, z0), new(x1, y0, z0), new(x1, y1, z0), new(x0, y1, z0));
        AddQuad(verts, tris, new(x1, y0, z1), new(x0, y0, z1), new(x0, y1, z1), new(x1, y1, z1));
        AddQuad(verts, tris, new(x0, y0, z0), new(x0, y0, z1), new(x0, y1, z1), new(x0, y1, z0));
        AddQuad(verts, tris, new(x1, y0, z1), new(x1, y0, z0), new(x1, y1, z0), new(x1, y1, z1));
        AddQuad(verts, tris, new(x0, y0, z1), new(x1, y0, z1), new(x1, y0, z0), new(x0, y0, z0));
        AddQuad(verts, tris, new(x0, y1, z0), new(x1, y1, z0), new(x1, y1, z1), new(x0, y1, z1));
    }

    private static void AddQuad(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var o = verts.Count;
        verts.Add(a);
        verts.Add(b);
        verts.Add(c);
        verts.Add(d);
        tris.Add(o);
        tris.Add(o + 1);
        tris.Add(o + 2);
        tris.Add(o);
        tris.Add(o + 2);
        tris.Add(o + 3);
    }
}
