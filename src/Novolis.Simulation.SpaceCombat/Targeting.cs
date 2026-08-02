using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

public static class Targeting
{
    public static CraftState? FindLockTarget(
        IReadOnlyList<CraftState> candidates,
        Vector3 origin,
        Vector3 forward,
        float maxDist = 90f,
        float minDist = 4f,
        float minDot = 0.55f)
    {
        CraftState? best = null;
        var bestScore = float.MaxValue;
        foreach (var enemy in candidates)
        {
            if (!enemy.Active || enemy.PlayerControlled)
                continue;

            var to = enemy.Position - origin;
            var dist = to.Length();
            if (dist > maxDist || dist < minDist)
                continue;

            var dir = to / dist;
            var dot = Vector3.Dot(forward, dir);
            if (dot < minDot)
                continue;

            var score = dist - dot * 20f;
            if (score >= bestScore)
                continue;

            bestScore = score;
            best = enemy;
        }

        return best;
    }
}
