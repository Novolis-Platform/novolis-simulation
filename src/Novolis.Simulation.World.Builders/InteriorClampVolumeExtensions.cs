using Novolis.Physics.Collision.Simple;
using Novolis.Simulation.World;

namespace Novolis.Simulation.World.Builders;

/// <summary>Maps simulation room bounds to physics interior clamp volumes.</summary>
public static class InteriorClampVolumeExtensions
{
    /// <summary>ToInteriorClamp operation.</summary>
    public static InteriorClampVolume ToInteriorClamp(this RoomInteriorBounds bounds) =>
        new()
        {
            MinX = bounds.MinX,
            MaxX = bounds.MaxX,
            MinY = bounds.MinY,
            MaxY = bounds.MaxY,
            MinZ = bounds.MinZ,
            MaxZ = bounds.MaxZ,
        };
}
