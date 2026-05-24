using System.Numerics;
using Novolis.Physics.Abstractions;

namespace Novolis.Simulation.World;

/// <summary>Heightfield over an axis-aligned XZ range box for ballistic and scene queries.</summary>
public sealed class BoundedHeightfield : IProjectileTerrainContact
{
    private readonly IHeightSampler _sampler;
    private readonly AxisAlignedRangeBox _range;

    /// <summary>BoundedHeightfield operation.</summary>
    /// <summary>Creates a square heightfield over [0, extent] on X and Z.</summary>
    public BoundedHeightfield(IHeightSampler sampler, float extentMeters)
    {
        _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
        _range = new AxisAlignedRangeBox(extentMeters);
    }

    /// <summary>BoundedHeightfield operation.</summary>
    /// <summary>Creates a heightfield over an explicit axis-aligned range box.</summary>
    public BoundedHeightfield(IHeightSampler sampler, in AxisAlignedRangeBox range)
    {
        _sampler = sampler ?? throw new ArgumentNullException(nameof(sampler));
        _range = range;
    }

    /// <summary>ExtentMeters.</summary>
    public float ExtentMeters => _range.ExtentMeters;

    /// <summary>IsInside operation.</summary>
    public bool IsInside(float x, float z) => _range.IsInside(x, z);

    /// <summary>Samples a value at the given coordinates.</summary>
    public float SampleHeight(float x, float z) => _sampler.SampleHeight(x, z);

    /// <summary>Attempts the operation and reports whether it succeeded.</summary>
    public bool TryHeightfieldContact(Vector3 position, float radius)
    {
        if (!IsInside(position.X, position.Z))
            return false;

        return position.Y <= _sampler.SampleHeight(position.X, position.Z) + radius;
    }

    /// <summary>Projects a point onto the surface.</summary>
    /// <inheritdoc />
    public Vector3 ProjectOntoSurface(Vector3 position, float surfaceEpsilon = 0.05f)
    {
        var x = System.Math.Clamp(position.X, 0f, _range.ExtentMeters);
        var z = System.Math.Clamp(position.Z, 0f, _range.ExtentMeters);
        var y = _sampler.SampleHeight(x, z) + surfaceEpsilon;
        return new Vector3(x, y, z);
    }

    /// <summary>Attempts the operation and reports whether it succeeded.</summary>
    public bool TrySegmentLeavesRange(
        Vector3 from,
        Vector3 to,
        out Vector3 hitPoint,
        out float fractionAlongSegment) =>
        _range.TrySegmentLeavesRange(from, to, out hitPoint, out fractionAlongSegment);
}
