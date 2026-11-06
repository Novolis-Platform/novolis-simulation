namespace Novolis.Simulation.World;

/// <summary>World extent and mesh resolution for heightfield scenes.</summary>
public sealed class WorldExtentOptions
{
    /// <summary>World extent along +X (east), meters.</summary>
    public float ExtentMeters { get; init; } = 2500f;

    /// <summary>World extent along +Z (north), meters. When zero, matches <see cref="ExtentMeters"/>.</summary>
    public float ExtentNorthMeters { get; init; }

    /// <summary>CollisionCells.</summary>
    public int CollisionCells { get; init; } = 256;

    /// <summary>DrawCells.</summary>
    public int DrawCells { get; init; } = 128;
}
