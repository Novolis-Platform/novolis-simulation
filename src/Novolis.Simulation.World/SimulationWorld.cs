using Novolis.Math.Arrays;

namespace Novolis.Simulation.World;

/// <summary>Planar occupancy grid and future entity composition root.</summary>
public sealed class SimulationWorld
{
    public SimulationWorld(DenseGrid<byte> occupancy, float cellSize = 1f)
    {
        Occupancy = occupancy ?? throw new ArgumentNullException(nameof(occupancy));
        CellSize = cellSize;
    }

    public DenseGrid<byte> Occupancy { get; }
    public float CellSize { get; }
}
