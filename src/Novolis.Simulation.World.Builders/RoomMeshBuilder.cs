using System.Numerics;

namespace Novolis.Simulation.World.Builders;

/// <summary>Shared axis-aligned box mesh helpers for occupancy room collision meshes.</summary>
public static class RoomMeshBuilder
{
    /// <summary>AppendBox operation.</summary>
    public static void AppendBox(
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

        AppendQuad(verts, tris, new(x0, y0, z0), new(x1, y0, z0), new(x1, y1, z0), new(x0, y1, z0));
        AppendQuad(verts, tris, new(x1, y0, z1), new(x0, y0, z1), new(x0, y1, z1), new(x1, y1, z1));
        AppendQuad(verts, tris, new(x0, y0, z0), new(x0, y0, z1), new(x0, y1, z1), new(x0, y1, z0));
        AppendQuad(verts, tris, new(x1, y0, z1), new(x1, y0, z0), new(x1, y1, z0), new(x1, y1, z1));
        AppendQuad(verts, tris, new(x0, y0, z1), new(x1, y0, z1), new(x1, y0, z0), new(x0, y0, z0));
        AppendQuad(verts, tris, new(x0, y1, z0), new(x1, y1, z0), new(x1, y1, z1), new(x0, y1, z1));
    }

    /// <summary>AppendQuad operation.</summary>
    public static void AppendQuad(List<Vector3> verts, List<int> tris, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
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
