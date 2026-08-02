using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

public static class HostileAi
{
    private const float OrbitInner = 32f;
    private const float OrbitOuter = 52f;
    private const float SeparationRadius = 14f;

    public static void Update(CraftState hostile, Vector3 playerPos, IReadOnlyList<CraftState> squadron, float dt)
    {
        if (!hostile.Active)
            return;

        hostile.FireCooldown = Math.Max(0, hostile.FireCooldown - dt);
        hostile.WeavePhase += dt * 1.8f;

        var toPlayer = playerPos - hostile.Position;
        var dist = toPlayer.Length();
        if (dist < 0.01f)
            return;

        var dir = toPlayer / dist;
        var right = Vector3.Normalize(Vector3.Cross(dir, Vector3.UnitY));
        if (right.LengthSquared() < 1e-4f)
            right = Vector3.UnitX;

        var separation = ComputeSeparation(hostile, squadron);
        var weave = right * MathF.Sin(hostile.WeavePhase) * 0.28f
                    + new Vector3(0, MathF.Cos(hostile.WeavePhase * 0.6f) * 0.12f, 0);

        Vector3 desired;
        float speed;
        if (dist < OrbitInner)
        {
            desired = -dir * 1.2f + separation + weave;
            speed = hostile.Profile.MaxSpeed * 0.26f;
        }
        else if (dist > OrbitOuter)
        {
            desired = dir * 0.85f + separation + weave * 0.5f;
            speed = hostile.Profile.MaxSpeed * 0.24f;
        }
        else
        {
            var tangent = Vector3.Normalize(Vector3.Cross(dir, Vector3.UnitY));
            desired = tangent * MathF.Sin(hostile.WeavePhase * 1.3f) + dir * 0.15f + separation + weave;
            speed = hostile.Profile.MaxSpeed * 0.28f;
        }

        if (desired.LengthSquared() < 1e-4f)
            desired = dir;

        hostile.Velocity = Vector3.Normalize(desired) * speed;
        hostile.Position += hostile.Velocity * dt;
        hostile.Speed = speed;

        var look = Vector3.Normalize(hostile.Velocity);
        if (look.LengthSquared() > 1e-4f)
        {
            hostile.Yaw = MathF.Atan2(look.X, look.Z);
            hostile.Pitch = MathF.Asin(Math.Clamp(look.Y, -1f, 1f));
        }
    }

    public static bool TryFire(CraftState hostile, Vector3 playerPos, int nearbyAllies)
    {
        if (!hostile.Active || hostile.FireCooldown > 0)
            return false;

        var toPlayer = playerPos - hostile.Position;
        var distSq = toPlayer.LengthSquared();
        if (distSq > 75f * 75f || distSq < 18f * 18f)
            return false;

        if (nearbyAllies >= 3 && Random.Shared.NextDouble() > 0.35)
            return false;

        hostile.FireCooldown = 1.1f + (float)Random.Shared.NextDouble() * 0.9f;
        return true;
    }

    public static void GetBoltVelocity(CraftState hostile, Vector3 playerPos, out Vector3 origin, out Vector3 velocity)
    {
        var toPlayer = playerPos - hostile.Position;
        var dir = Vector3.Normalize(toPlayer);
        origin = hostile.Position + dir * 1.2f;
        velocity = dir * 88f;
    }

    public static void SpawnNear(CraftState hostile, Vector3 playerPos, Vector3 playerForward, Random rng)
    {
        hostile.Active = true;
        hostile.ResetVitals();
        hostile.WeavePhase = (float)rng.NextDouble() * MathF.Tau;
        hostile.FireCooldown = 0.5f + (float)rng.NextDouble();

        var forward = Vector3.Normalize(playerForward);
        var right = Vector3.Normalize(Vector3.Cross(forward, Vector3.UnitY));
        if (right.LengthSquared() < 1e-4f)
            right = Vector3.UnitX;

        var dist = 95f + (float)rng.NextDouble() * 55f;
        var lateral = ((float)rng.NextDouble() * 2f - 1f) * 48f;
        var vertical = (float)(rng.NextDouble() * 10 - 5);
        hostile.Position = playerPos + forward * dist + right * lateral + new Vector3(0, vertical, 0);
        hostile.Velocity = Vector3.Zero;
        hostile.Speed = hostile.Profile.MinSpeed;
    }

    private static Vector3 ComputeSeparation(CraftState self, IReadOnlyList<CraftState> squadron)
    {
        var push = Vector3.Zero;
        foreach (var other in squadron)
        {
            if (!other.Active || ReferenceEquals(other, self))
                continue;

            var offset = self.Position - other.Position;
            var len = offset.Length();
            if (len < SeparationRadius && len > 0.01f)
                push += offset / len * (SeparationRadius - len) * 0.55f;
        }

        return push;
    }
}
