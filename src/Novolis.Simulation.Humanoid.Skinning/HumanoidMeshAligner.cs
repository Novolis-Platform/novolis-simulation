using System.Numerics;
using Novolis.Math.Geometry;

namespace Novolis.Simulation.Humanoid.Skinning;

/// <summary>
/// Fits an unrigged character mesh to a <see cref="HumanoidBindPose"/> frame
/// (uniform scale to height, feet on Y=0, XZ centered on hips).
/// </summary>
public static class HumanoidMeshAligner
{
    /// <summary>
    /// Mutates <paramref name="mesh"/> so its vertical extent matches <paramref name="bind"/>.HeightMeters,
    /// minimum Y is 0, and XZ centroid sits on the bind hips XZ.
    /// </summary>
    public static void FitToBindPose(EditableMesh mesh, HumanoidBindPose bind)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        ArgumentNullException.ThrowIfNull(bind);
        if (mesh.VertexCount == 0)
            return;

        var (min, max) = Bounds(mesh);
        var size = max - min;
        var height = size.Y;
        if (height < 1e-5f)
            return;

        var scale = bind.HeightMeters / height;
        var centerXz = new Vector3((min.X + max.X) * 0.5f, 0f, (min.Z + max.Z) * 0.5f);
        var hips = bind[HumanoidBone.Hips];

        // Translate so feet≈0 and XZ at origin, scale about origin, then place XZ on hips.
        var xf =
            Matrix4x4.CreateTranslation(-centerXz.X, -min.Y, -centerXz.Z) *
            Matrix4x4.CreateScale(scale) *
            Matrix4x4.CreateTranslation(hips.X, 0f, hips.Z);

        mesh.Transform(xf);
    }

    /// <summary>Returns a new triangle mesh fitted to the bind pose.</summary>
    public static TriangleMesh FitToBindPose(TriangleMesh mesh, HumanoidBindPose bind)
    {
        ArgumentNullException.ThrowIfNull(mesh);
        var editable = EditableMesh.FromTriangleMesh(mesh);
        FitToBindPose(editable, bind);
        return editable.ToTriangleMesh();
    }

    private static (Vector3 Min, Vector3 Max) Bounds(EditableMesh mesh)
    {
        var min = new Vector3(float.MaxValue);
        var max = new Vector3(float.MinValue);
        for (var i = 0; i < mesh.VertexCount; i++)
        {
            var v = mesh.Vertices[i];
            min = Vector3.Min(min, v);
            max = Vector3.Max(max, v);
        }

        return (min, max);
    }
}
