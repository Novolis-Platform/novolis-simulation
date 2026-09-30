using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

public static class BoltPools
{
    public static bool TrySpawn(LaserBolt[] pool, Vector3 origin, Vector3 velocity, float life, bool fromPlayer, float damage = 0.55f)
    {
        foreach (var bolt in pool)
        {
            if (bolt.Active)
                continue;
            bolt.Active = true;
            bolt.Position = origin;
            bolt.Velocity = velocity;
            bolt.Life = life;
            bolt.FromPlayer = fromPlayer;
            bolt.Damage = damage;
            return true;
        }

        return false;
    }

    public static void Update(LaserBolt[] pool, float dt, Vector3 playerPos, float maxDist)
    {
        foreach (var bolt in pool)
        {
            if (!bolt.Active)
                continue;
            bolt.Position += bolt.Velocity * dt;
            bolt.Life -= dt;
            if (bolt.Life <= 0 || Vector3.Distance(bolt.Position, playerPos) > maxDist)
                bolt.Active = false;
        }
    }
}
