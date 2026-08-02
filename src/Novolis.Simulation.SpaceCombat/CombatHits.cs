using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

public static class CombatHits
{
    public static bool SegmentHitsSphere(Vector3 segStart, Vector3 segEnd, Vector3 center, float radius)
    {
        var ab = segEnd - segStart;
        var ac = center - segStart;
        var abLenSq = ab.LengthSquared();
        if (abLenSq < 1e-8f)
            return Vector3.Distance(segStart, center) <= radius;

        var t = Math.Clamp(Vector3.Dot(ac, ab) / abLenSq, 0f, 1f);
        var closest = segStart + ab * t;
        return Vector3.Distance(closest, center) <= radius;
    }
}
