using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>Two-bone IK for limbs (arms / legs).</summary>
public static class TwoBoneIk
{
    /// <summary>
    /// Places the mid joint so <paramref name="root"/> → mid → <paramref name="target"/>
    /// match <paramref name="upperLength"/> / <paramref name="lowerLength"/>, bending toward <paramref name="poleVector"/>.
    /// </summary>
    public static Vector3 SolveMid(
        Vector3 root,
        Vector3 target,
        float upperLength,
        float lowerLength,
        Vector3 poleVector)
    {
        var toTarget = target - root;
        var dist = toTarget.Length();
        if (dist < 1e-5f)
            return root + new Vector3(0f, -upperLength, 0f);

        var dir = toTarget / dist;
        var maxReach = upperLength + lowerLength - 0.01f;
        dist = Math.Min(dist, maxReach);
        var minReach = MathF.Abs(upperLength - lowerLength) + 0.01f;
        dist = Math.Max(dist, minReach);

        var cosAngle = (upperLength * upperLength + dist * dist - lowerLength * lowerLength) /
                       (2f * upperLength * dist);
        cosAngle = Math.Clamp(cosAngle, -1f, 1f);
        var angle = MathF.Acos(cosAngle);

        var axis = Vector3.Cross(dir, poleVector);
        if (axis.LengthSquared() < 1e-6f)
            axis = Vector3.Cross(dir, Vector3.UnitY);
        if (axis.LengthSquared() < 1e-6f)
            axis = Vector3.UnitX;
        axis = Vector3.Normalize(axis);

        var upperDir = Rotate(dir, axis, angle);
        return root + upperDir * upperLength;
    }

    /// <summary>Applies two-bone IK to a limb chain and writes mid + end world positions into <paramref name="world"/>.</summary>
    public static void ApplyLimb(
        HumanoidWorldPose world,
        HumanoidBone root,
        HumanoidBone mid,
        HumanoidBone end,
        Vector3 target,
        float upperLength,
        float lowerLength,
        Vector3 poleVector)
    {
        var rootPos = world.Position(root);
        var midPos = SolveMid(rootPos, target, upperLength, lowerLength, poleVector);
        world.Set(mid, midPos, world.Rotation(mid));
        world.Set(end, target, world.Rotation(end));
    }

    private static Vector3 Rotate(Vector3 v, Vector3 axis, float angle)
    {
        var c = MathF.Cos(angle);
        var s = MathF.Sin(angle);
        return v * c + Vector3.Cross(axis, v) * s + axis * Vector3.Dot(axis, v) * (1f - c);
    }
}
