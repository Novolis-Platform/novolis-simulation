using Novolis.Physics.Collision.Simple;
using Novolis.Physics.Numerics;

namespace Novolis.Simulation.World.Builders;

/// <summary>Builds a <see cref="BvhStaticWorld"/> from an occupancy grid (XZ plane, +Y up).</summary>
public static class OccupancyColumnMeshBuilder
{
    public static BvhStaticWorld FromWallGrid(
        uint width,
        uint height,
        ReadOnlySpan<byte> cells,
        double cellSize = 1.0,
        double wallHeight = 2.0)
    {
        var verts = new List<Vector3d>();
        var tris = new List<int>();

        for (var y = 0u; y < height; y++)
        for (var x = 0u; x < width; x++)
        {
            var index = (int)(y * width + x);
            if (index >= cells.Length || cells[index] == 0)
                continue;

            var cx = (x + 0.5) * cellSize;
            var cz = (y + 0.5) * cellSize;
            var h = wallHeight * 0.5;
            var hx = cellSize * 0.5;
            AddBox(verts, tris, cx, h, cz, hx, h, hx);
        }

        return new BvhStaticWorld(new StaticTriangleMesh(verts.ToArray(), tris.ToArray()));
    }

    private static void AddBox(
        List<Vector3d> verts,
        List<int> tris,
        double cx,
        double cy,
        double cz,
        double hx,
        double hy,
        double hz)
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

    private static void AddQuad(List<Vector3d> verts, List<int> tris, Vector3d a, Vector3d b, Vector3d c, Vector3d d)
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
