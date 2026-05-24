namespace Novolis.Simulation.World;

/// <summary>World extent and mesh resolution for heightfield scenes.</summary>
public sealed class WorldExtentOptions
{
    /// <summary>ExtentMeters.</summary>
    public float ExtentMeters { get; init; } = 2500f;

    /// <summary>CollisionCells.</summary>
    public int CollisionCells { get; init; } = 256;

    /// <summary>DrawCells.</summary>
    public int DrawCells { get; init; } = 128;
}
