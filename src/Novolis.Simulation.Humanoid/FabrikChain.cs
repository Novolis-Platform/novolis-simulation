using System.Numerics;

namespace Novolis.Simulation.Humanoid;

/// <summary>
/// FABRIK (Forward And Backward Reaching Inverse Kinematics) on an open joint chain.
/// Operates on BCL positions only — no humanoid bone schema.
/// </summary>
public static class FabrikChain
{
    /// <summary>
    /// Solves so the last joint approaches <paramref name="target"/> while preserving
    /// <paramref name="lengths"/> between consecutive joints.
    /// </summary>
    /// <param name="positions">Joint positions (length ≥ 2); mutated in place.</param>
    /// <param name="lengths">Rest length between joint i and i+1 (length = positions.Length - 1).</param>
    /// <param name="target">End-effector goal.</param>
    /// <param name="pinRoot">When true, joint 0 is restored after each backward pass.</param>
    /// <param name="maxIterations">Iteration budget.</param>
    /// <param name="tolerance">Stop when ‖end − target‖ ≤ tolerance.</param>
    /// <returns>True when the end effector is within <paramref name="tolerance"/> of the target.</returns>
    public static bool Solve(
        Span<Vector3> positions,
        ReadOnlySpan<float> lengths,
        Vector3 target,
        bool pinRoot = true,
        int maxIterations = 16,
        float tolerance = 1e-4f)
    {
        if (positions.Length < 2)
            throw new ArgumentException("Chain needs at least two joints.", nameof(positions));
        if (lengths.Length != positions.Length - 1)
            throw new ArgumentException("lengths.Length must equal positions.Length - 1.", nameof(lengths));
        if (maxIterations < 1)
            throw new ArgumentOutOfRangeException(nameof(maxIterations));

        var root = positions[0];
        var totalLength = 0f;
        for (var i = 0; i < lengths.Length; i++)
        {
            if (lengths[i] < 0f)
                throw new ArgumentOutOfRangeException(nameof(lengths), "Segment lengths must be non-negative.");
            totalLength += lengths[i];
        }

        var toTarget = target - root;
        var dist = toTarget.Length();
        if (dist > totalLength)
        {
            // Unreachable: stretch along root→target.
            if (dist < 1e-8f)
                return false;
            var dir = toTarget / dist;
            var cursor = root;
            positions[0] = root;
            for (var i = 0; i < lengths.Length; i++)
            {
                cursor += dir * lengths[i];
                positions[i + 1] = cursor;
            }

            return false;
        }

        var tolSq = tolerance * tolerance;
        for (var iter = 0; iter < maxIterations; iter++)
        {
            if ((positions[^1] - target).LengthSquared() <= tolSq)
                return true;

            // Forward: set tip to target, then constrain toward root.
            positions[^1] = target;
            for (var i = positions.Length - 2; i >= 0; i--)
                positions[i] = ConstrainDistance(positions[i + 1], positions[i], lengths[i]);

            // Backward: pin root, then constrain toward tip.
            if (pinRoot)
                positions[0] = root;
            for (var i = 0; i < lengths.Length; i++)
                positions[i + 1] = ConstrainDistance(positions[i], positions[i + 1], lengths[i]);
        }

        return (positions[^1] - target).LengthSquared() <= tolSq;
    }

    /// <summary>Places <paramref name="point"/> on the sphere of radius <paramref name="length"/> around <paramref name="anchor"/>.</summary>
    public static Vector3 ConstrainDistance(Vector3 anchor, Vector3 point, float length)
    {
        var delta = point - anchor;
        var d = delta.Length();
        if (d < 1e-8f)
            return anchor + Vector3.UnitY * length;

        return anchor + delta * (length / d);
    }
}