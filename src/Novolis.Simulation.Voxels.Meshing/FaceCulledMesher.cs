using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Voxels.Meshing;

/// <summary>Emits one quad (two triangles) per exposed voxel face.</summary>
public static class FaceCulledMesher
{
    static readonly (int Dx, int Dy, int Dz, Vector3[] Corners)[] Faces =
    [
        // +X
        (1, 0, 0, [new(1, 0, 0), new(1, 0, 1), new(1, 1, 1), new(1, 1, 0)]),
        // -X
        (-1, 0, 0, [new(0, 0, 1), new(0, 0, 0), new(0, 1, 0), new(0, 1, 1)]),
        // +Y
        (0, 1, 0, [new(0, 1, 0), new(1, 1, 0), new(1, 1, 1), new(0, 1, 1)]),
        // -Y
        (0, -1, 0, [new(0, 0, 1), new(1, 0, 1), new(1, 0, 0), new(0, 0, 0)]),
        // +Z
        (0, 0, 1, [new(0, 0, 1), new(0, 1, 1), new(1, 1, 1), new(1, 0, 1)]),
        // -Z
        (0, 0, -1, [new(1, 0, 0), new(1, 1, 0), new(0, 1, 0), new(0, 0, 0)]),
    ];

    /// <summary>Builds a mesh for one chunk using neighbor queries on <paramref name="world"/>.</summary>
    public static EditableMesh Build(ChunkedVoxelWorld world, ChunkCoord3 coord)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!world.TryGetChunk(coord, out var chunk) || chunk.IsEmpty)
            return new EditableMesh();

        var mesh = new EditableMesh();
        var ox = coord.X * VoxelChunk.Size;
        var oy = coord.Y * VoxelChunk.Size;
        var oz = coord.Z * VoxelChunk.Size;

        for (var ly = 0; ly < VoxelChunk.Size; ly++)
        for (var lz = 0; lz < VoxelChunk.Size; lz++)
        for (var lx = 0; lx < VoxelChunk.Size; lx++)
        {
            if (chunk.Get(lx, ly, lz) == 0)
                continue;
            var wx = ox + lx;
            var wy = oy + ly;
            var wz = oz + lz;
            foreach (var (dx, dy, dz, corners) in Faces)
            {
                if (world.IsSolid(wx + dx, wy + dy, wz + dz))
                    continue;
                AddQuad(mesh, wx, wy, wz, corners);
            }
        }

        return mesh;
    }

    /// <summary>Triangle count for diagnostics.</summary>
    public static int CountExposedFaces(ChunkedVoxelWorld world, ChunkCoord3 coord)
    {
        var mesh = Build(world, coord);
        return mesh.TriangleCount / 2;
    }

    static void AddQuad(EditableMesh mesh, int wx, int wy, int wz, Vector3[] corners)
    {
        var i0 = mesh.AddVertex(new Vector3(wx, wy, wz) + corners[0]);
        var i1 = mesh.AddVertex(new Vector3(wx, wy, wz) + corners[1]);
        var i2 = mesh.AddVertex(new Vector3(wx, wy, wz) + corners[2]);
        var i3 = mesh.AddVertex(new Vector3(wx, wy, wz) + corners[3]);
        mesh.AddTriangle(i0, i1, i2);
        mesh.AddTriangle(i0, i2, i3);
    }
}
