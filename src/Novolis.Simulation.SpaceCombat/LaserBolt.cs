using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

public sealed class LaserBolt
{
    public Vector3 Position;
    public Vector3 Velocity;
    public float Life;
    public bool Active;
    public bool FromPlayer = true;
    public float Damage = 0.55f;
}
