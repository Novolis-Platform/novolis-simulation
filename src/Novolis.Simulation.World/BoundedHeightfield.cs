using System.Numerics;
using Novolis.Physics.Abstractions;

namespace Novolis.Simulation.World;

/// <summary>Heightfield over an axis-aligned XZ range box for ballistic and scene queries.</summary>
public sealed class BoundedHeightfield : IProjectileTerrainContact
{
    private readonly IHeightSampler _sampler;
    private readonly AxisAlignedRangeBox _range;

    public BoundedHeightfield(IHeightSampler sampler, float extentMeters)
    {
        _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
        _range = new AxisAlignedRangeBox(extentMeters);
    }

    public BoundedHeightfield(IHeightSampler sampler, in AxisAlignedRangeBox range)
    {
        _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
        _range = range;
    }

    public float ExtentMeters => _range.ExtentMeters;

    public bool IsInside(float x, float z) => _range.IsInside(x, z);

    public float SampleHeight(float x, float z) => _sampler.SampleHeight(x, z);

    public bool TryHeightfieldContact(Vector3 position, float radius)
    {
        if (!IsInside(position.X, position.Z))
            return false;

        return position.Y <= _sampler.SampleHeight(position.X, position.Z) + radius;
    }

    public Vector3 ProjectOntoSurface(Vector3 position, float surfaceEpsilon = 0.05f)
    {
        var x = System.Math.Clamp(position.X, 0f, _range.ExtentMeters);
        var z = System.Math.Clamp(position.Z, 0f, _range.ExtentMeters);
        var y = _sampler.SampleHeight(x, z) + surfaceEpsilon;
        return new Vector3(x, y, z);
    }

    public bool TrySegmentLeavesRange(
        Vector3 from,
        Vector3 to,
        out Vector3 hitPoint,
        out float fractionAlongSegment) =>
        _range.TrySegmentLeavesRange(from, to, out hitPoint, out fractionAlongSegment);
}
