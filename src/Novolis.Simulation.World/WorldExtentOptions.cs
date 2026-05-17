namespace Novolis.Simulation.World;

/// <summary>World extent and mesh resolution for heightfield scenes.</summary>
public sealed class WorldExtentOptions
{
    public float ExtentMeters { get; init; } = 2500f;

    public int CollisionCells { get; init; } = 256;

    public int DrawCells { get; init; } = 128;
}
