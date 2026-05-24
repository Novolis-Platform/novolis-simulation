using System.Numerics;
using Novolis.Physics.Abstractions;
using Novolis.Physics.Collision.Simple;
using Novolis.Simulation.World;

namespace Novolis.Simulation.World.Builders;

/// <summary>Builds collision and decimated draw meshes from a height sampler.</summary>
public static class HeightfieldMeshBuilder
{
    /// <summary>Builds the configured instance.</summary>
    public static HeightfieldBuildResult Build(IHeightSampler sampler, WorldExtentOptions options)
    {
        var extent = options.ExtentMeters;
        var collisionCells = options.CollisionCells;
        var drawCells = options.DrawCells;

        var verts = new Vector3[(collisionCells + 1) * (collisionCells + 1)];
        var tris = new List<int>(collisionCells * collisionCells * 6);

        for (var z = 0; z <= collisionCells; z++)
        for (var x = 0; x <= collisionCells; x++)
        {
            var fx = x / (float)collisionCells * extent;
            var fz = z / (float)collisionCells * extent;
            verts[z * (collisionCells + 1) + x] = new Vector3(fx, sampler.SampleHeight(fx, fz), fz);
        }

        for (var z = 0; z < collisionCells; z++)
        for (var x = 0; x < collisionCells; x++)
        {
            var i00 = z * (collisionCells + 1) + x;
            var i10 = i00 + 1;
            var i01 = i00 + (collisionCells + 1);
            var i11 = i01 + 1;
            tris.Add(i00);
            tris.Add(i10);
            tris.Add(i01);
            tris.Add(i10);
            tris.Add(i11);
            tris.Add(i01);
        }

        var collision = new BvhStaticWorld(new StaticTriangleMesh(verts, tris.ToArray()));

        var drawVertices = new Vector3[(drawCells + 1) * (drawCells + 1)];
        for (var z = 0; z <= drawCells; z++)
        for (var x = 0; x <= drawCells; x++)
        {
            var fx = x / (float)drawCells * extent;
            var fz = z / (float)drawCells * extent;
            drawVertices[z * (drawCells + 1) + x] = new Vector3(fx, sampler.SampleHeight(fx, fz), fz);
        }

        return new HeightfieldBuildResult
        {
            Collision = collision,
            DrawVertices = drawVertices,
            DrawCells = drawCells,
        };
    }
}
