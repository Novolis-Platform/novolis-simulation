namespace Novolis.Simulation.Tiles;

/// <summary>Axis for wall edges on the grid.</summary>
public enum WallAxis : byte
{
    /// <summary>Edge parallel to X (separates Z and Z+1).</summary>
    AlongX = 0,

    /// <summary>Edge parallel to Z (separates X and X+1).</summary>
    AlongZ = 1
}
