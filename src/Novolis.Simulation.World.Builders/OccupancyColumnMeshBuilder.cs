using Novolis.Physics.Collision.Simple;

namespace Novolis.Simulation.World.Builders;

/// <summary>Builds a <see cref="BvhStaticWorld"/> from an occupancy grid (XZ plane, +Y up).</summary>
public static class OccupancyColumnMeshBuilder
{
    /// <summary>FromWallGrid operation.</summary>
    public static BvhStaticWorld FromWallGrid(
        uint width,
        uint height,
        ReadOnlySpan<byte> cells,
        float cellSize = 1f,
        float wallHeight = 2f) =>
        OccupancyEnclosedRoomMeshBuilder.FromWallGrid(
            width,
            height,
            cells,
            cellSize,
            wallHeight,
            includeFloor: false,
            includeCeiling: false);
}
