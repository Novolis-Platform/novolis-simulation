using Novolis.Math.Arrays;

namespace Novolis.Simulation.World;

/// <summary>Planar occupancy grid and future entity composition root.</summary>
public sealed class SimulationWorld
{
    /// <summary>Creates a world from a planar occupancy grid.</summary>
    public SimulationWorld(DenseGrid<byte> occupancy, float cellSize = 1f)
    {
        Occupancy = occupancy ?? throw new ArgumentNullException(nameof(occupancy));
        CellSize = cellSize;
    }

    /// <summary>Walkability grid (0 = open).</summary>
    public DenseGrid<byte> Occupancy { get; }

    /// <summary>World meters per occupancy cell.</summary>
    public float CellSize { get; }
}
