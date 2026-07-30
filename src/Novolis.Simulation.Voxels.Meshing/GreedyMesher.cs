using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Voxels.Meshing;

/// <summary>
/// Greedy meshing: merges coplanar same-id faces on each axis slice.
/// Uses <see cref="ChunkedVoxelWorld"/> for neighbor air tests at chunk borders.
/// </summary>
public static class GreedyMesher
{
    /// <summary>Builds a greedy mesh for one chunk.</summary>
    public static EditableMesh Build(ChunkedVoxelWorld world, ChunkCoord3 coord)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!world.TryGetChunk(coord, out var chunk) || chunk.IsEmpty)
            return new EditableMesh();

        var mesh = new EditableMesh();
        var ox = coord.X * VoxelChunk.Size;
        var oy = coord.Y * VoxelChunk.Size;
        var oz = coord.Z * VoxelChunk.Size;
        const int n = VoxelChunk.Size;

        // dims: 0=X, 1=Y, 2=Z — process both face signs via q loop
        for (var dim = 0; dim < 3; dim++)
        {
            var u = (dim + 1) % 3;
            var v = (dim + 2) % 3;
            var x = new int[3];
            var q = new int[3];
            q[dim] = 1;

            var mask = new ushort[n * n];

            for (x[dim] = -1; x[dim] < n;)
            {
                var nMask = 0;
                for (x[v] = 0; x[v] < n; x[v]++)
                for (x[u] = 0; x[u] < n; x[u]++, nMask++)
                {
                    var a = InChunk(x) ? world.GetBlock(ox + x[0], oy + x[1], oz + x[2]) : (ushort)0;
                    var bx = x[0] + q[0];
                    var by = x[1] + q[1];
                    var bz = x[2] + q[2];
                    var b = world.GetBlock(ox + bx, oy + by, oz + bz);

                    if ((a != 0) == (b != 0))
                        mask[nMask] = 0;
                    else if (a != 0)
                        mask[nMask] = a; // face toward +dim
                    else
                        mask[nMask] = (ushort)(b | 0x8000); // high bit = face toward -dim
                }

                x[dim]++;

                nMask = 0;
                for (var j = 0; j < n; j++)
                for (var i = 0; i < n;)
                {
                    var c = mask[nMask];
                    if (c == 0)
                    {
                        i++;
                        nMask++;
                        continue;
                    }

                    var width = 1;
                    while (i + width < n && mask[nMask + width] == c)
                        width++;

                    var height = 1;
                    var done = false;
                    while (j + height < n && !done)
                    {
                        for (var k = 0; k < width; k++)
                        {
                            if (mask[nMask + k + height * n] != c)
                            {
                                done = true;
                                break;
                            }
                        }

                        if (!done)
                            height++;
                    }

                    for (var h = 0; h < height; h++)
                    for (var w = 0; w < width; w++)
                        mask[nMask + w + h * n] = 0;

                    EmitQuad(mesh, dim, u, v, x, q, i, j, width, height, c, ox, oy, oz);
                    i += width;
                    nMask += width;
                }
            }
        }

        return mesh;

        static bool InChunk(int[] p) =>
            (uint)p[0] < n && (uint)p[1] < n && (uint)p[2] < n;
    }

    static void EmitQuad(
        EditableMesh mesh,
        int dim,
        int u,
        int v,
        int[] x,
        int[] q,
        int i,
        int j,
        int w,
        int h,
        ushort code,
        int ox,
        int oy,
        int oz)
    {
        var flip = (code & 0x8000) != 0;
        var du = new int[3];
        var dv = new int[3];
        du[u] = w;
        dv[v] = h;

        // Plane at current x[dim] (already incremented past the face in the classic algo).
        // Face sits between (x[dim]-1) and x[dim].
        var origin = new int[3];
        origin[0] = x[0];
        origin[1] = x[1];
        origin[2] = x[2];
        origin[dim]--;
        origin[u] = i;
        origin[v] = j;

        Vector3 V(int[] p) => new(ox + p[0], oy + p[1], oz + p[2]);

        var p0 = new[] { origin[0], origin[1], origin[2] };
        var p1 = new[] { origin[0] + du[0], origin[1] + du[1], origin[2] + du[2] };
        var p2 = new[] { origin[0] + du[0] + dv[0], origin[1] + du[1] + dv[1], origin[2] + du[2] + dv[2] };
        var p3 = new[] { origin[0] + dv[0], origin[1] + dv[1], origin[2] + dv[2] };

        // Offset by q when face is on the + side of the solid (not flip)
        if (!flip)
        {
            for (var k = 0; k < 3; k++)
            {
                p0[k] += q[k];
                p1[k] += q[k];
                p2[k] += q[k];
                p3[k] += q[k];
            }
        }

        int a, b, c, d;
        if (flip)
        {
            a = mesh.AddVertex(V(p0));
            b = mesh.AddVertex(V(p3));
            c = mesh.AddVertex(V(p2));
            d = mesh.AddVertex(V(p1));
        }
        else
        {
            a = mesh.AddVertex(V(p0));
            b = mesh.AddVertex(V(p1));
            c = mesh.AddVertex(V(p2));
            d = mesh.AddVertex(V(p3));
        }

        mesh.AddTriangle(a, b, c);
        mesh.AddTriangle(a, c, d);
    }
}
