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
        var mid = root + upperDir * upperLength;
        return EnforceBendSide(root, mid, root + dir * dist, poleVector);
    }

    /// <summary>
    /// Reflects <paramref name="mid"/> across the root→end axis when it lies on the wrong side
    /// of the pole (prevents inverted elbows/knees).
    /// </summary>
    public static Vector3 EnforceBendSide(Vector3 root, Vector3 mid, Vector3 end, Vector3 poleVector)
    {
        var toEnd = end - root;
        if (toEnd.LengthSquared() < 1e-10f)
            return mid;

        var preferred = Vector3.Cross(toEnd, poleVector);
        if (preferred.LengthSquared() < 1e-10f)
            return mid;

        var toMid = mid - root;
        var actual = Vector3.Cross(toEnd, toMid);
        if (actual.LengthSquared() < 1e-12f)
            return mid;

        if (Vector3.Dot(actual, preferred) >= 0f)
            return mid;

        var dir = Vector3.Normalize(toEnd);
        var along = Vector3.Dot(toMid, dir) * dir;
        var perp = toMid - along;
        return root + along - perp;
    }

    /// <summary>
    /// Applies two-bone IK and writes mid/end (and root rotation) so LBS bones aim along the solved limb.
    /// Positions alone are not enough — CPU skin deformers use world rotations.
    /// </summary>
    public static void ApplyLimb(
        HumanoidWorldPose world,
        HumanoidBindPose bind,
        HumanoidBone root,
        HumanoidBone mid,
        HumanoidBone end,
        Vector3 target,
        float upperLength,
        float lowerLength,
        Vector3 poleVector)
    {
        ArgumentNullException.ThrowIfNull(world);
        ArgumentNullException.ThrowIfNull(bind);

        var rootPos = world.Position(root);
        var endPos = ClampReach(rootPos, target, upperLength, lowerLength);
        var midPos = SolveMid(rootPos, endPos, upperLength, lowerLength, poleVector);
        midPos = EnforceBendSide(rootPos, midPos, endPos, poleVector);

        var bindUpper = bind[mid] - bind[root];
        var bindLower = bind[end] - bind[mid];
        var curUpper = midPos - rootPos;
        var curLower = endPos - midPos;

        var rootRot = FromToRotation(bindUpper, curUpper);
        var midRot = FromToRotation(bindLower, curLower);
        // Hand follows forearm aim so grip verts rotate with the palm.
        var endRot = midRot;

        world.Set(root, rootPos, rootRot);
        world.Set(mid, midPos, midRot);
        world.Set(end, endPos, endRot);
    }

    /// <summary>Legacy overload — prefers the bind-aware overload for skinning.</summary>
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
        var endPos = ClampReach(rootPos, target, upperLength, lowerLength);
        var midPos = SolveMid(rootPos, endPos, upperLength, lowerLength, poleVector);
        midPos = EnforceBendSide(rootPos, midPos, endPos, poleVector);
        world.Set(mid, midPos, world.Rotation(mid));
        world.Set(end, endPos, world.Rotation(end));
    }

    /// <summary>Pulls <paramref name="target"/> onto the reachable annulus so limbs never stretch.</summary>
    public static Vector3 ClampReach(Vector3 root, Vector3 target, float upperLength, float lowerLength)
    {
        var toTarget = target - root;
        var dist = toTarget.Length();
        var maxReach = upperLength + lowerLength - 0.01f;
        var minReach = MathF.Abs(upperLength - lowerLength) + 0.01f;
        if (dist < 1e-5f)
            return root + new Vector3(0f, -MathF.Max(minReach, upperLength * 0.5f), 0f);

        var dir = toTarget / dist;
        dist = Math.Clamp(dist, minReach, maxReach);
        return root + dir * dist;
    }

    /// <summary>Shortest rotation taking <paramref name="from"/> onto <paramref name="to"/>.</summary>
    public static Quaternion FromToRotation(Vector3 from, Vector3 to)
    {
        if (from.LengthSquared() < 1e-10f || to.LengthSquared() < 1e-10f)
            return Quaternion.Identity;

        from = Vector3.Normalize(from);
        to = Vector3.Normalize(to);
        var dot = Vector3.Dot(from, to);
        if (dot > 0.999999f)
            return Quaternion.Identity;
        if (dot < -0.999999f)
        {
            var axis = Vector3.Cross(Vector3.UnitX, from);
            if (axis.LengthSquared() < 1e-8f)
                axis = Vector3.Cross(Vector3.UnitY, from);
            return Quaternion.CreateFromAxisAngle(Vector3.Normalize(axis), MathF.PI);
        }

        var cross = Vector3.Cross(from, to);
        var q = new Quaternion(cross.X, cross.Y, cross.Z, 1f + dot);
        return Quaternion.Normalize(q);
    }

    private static Vector3 Rotate(Vector3 v, Vector3 axis, float angle)
    {
        var c = MathF.Cos(angle);
        var s = MathF.Sin(angle);
        return v * c + Vector3.Cross(axis, v) * s + axis * Vector3.Dot(axis, v) * (1f - c);
    }
}
