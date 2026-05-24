using System.Numerics;

using Novolis.Simulation.Racing.Tracks;
using Novolis.Simulation.Racing.Tests.Infrastructure;

using TUnit.Assertions;

namespace Novolis.Simulation.Racing.Tests;

public class CatmullRomSplineSamplerTests : BaseTest
{
    private readonly CatmullRomSplineSampler _sampler = new();

    private static readonly SplineLoop _circleSpline =
        TrackSpecs.Circle("test", 120, 50, new Vector2(60, 25), 28f, 16f, 4.5, 16, 0, 1).CenterLine;

    private static readonly SplineLoop _squareSpline = new(new[]
    {
        new Vector2(0, 0), new Vector2(10, 0), new Vector2(10, 10), new Vector2(0, 10)
    });

    private static readonly SplineLoop _triangleSpline = new(new[]
    {
        new Vector2(0, 0), new Vector2(10, 0), new Vector2(5, 10)
    });

    [Test]
    public async Task Sample_AtT0_ReturnsPositionNearFirstControlPoint()
    {
        var s = _sampler.Sample(_circleSpline, 0.0);
        await Assert.That(s.Position.X).IsEqualTo(88f).Within(0.5f);
        await Assert.That(s.Position.Y).IsEqualTo(25f).Within(0.5f);
    }

    [Test]
    public async Task Sample_AtT025_IsApproximatelyAtQuarterPoint()
    {
        var s = _sampler.Sample(_circleSpline, 0.25);
        await Assert.That(s.Position.X).IsEqualTo(60f).Within(0.5f);
        await Assert.That(s.Position.Y).IsEqualTo(41f).Within(0.5f);
    }

    [Test]
    public async Task Sample_AtT05_IsApproximatelyAtHalfwayPoint()
    {
        var s = _sampler.Sample(_circleSpline, 0.5);
        await Assert.That(s.Position.X).IsEqualTo(32f).Within(0.5f);
        await Assert.That(s.Position.Y).IsEqualTo(25f).Within(0.5f);
    }

    [Test]
    public async Task Sample_AtT075_IsApproximatelyAtThreeQuarterPoint()
    {
        var s = _sampler.Sample(_circleSpline, 0.75);
        await Assert.That(s.Position.X).IsEqualTo(60f).Within(0.5f);
        await Assert.That(s.Position.Y).IsEqualTo(9f).Within(0.5f);
    }

    [Test]
    public async Task Sample_AtT0999_IsCloseToT0()
    {
        var s0 = _sampler.Sample(_circleSpline, 0.0);
        var sEnd = _sampler.Sample(_circleSpline, 0.999);
        await Assert.That(Vector2.Distance(s0.Position, sEnd.Position)).IsLessThan(2f);
    }

    [Test]
    public async Task SampleEvenly_ReturnsCorrectCount()
    {
        foreach (int n in new[] { 4, 10, 50, 100, 1000 })
            await Assert.That(_sampler.SampleEvenly(_circleSpline, n).Count).IsEqualTo(n);
    }

    [Test]
    public async Task Tangent_IsUnitLength_ForAllSampledPoints()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 100);
        foreach (var s in samples)
            await Assert.That(s.Tangent.Length()).IsEqualTo(1f).Within(1e-5f);
    }

    [Test]
    public async Task Normal_IsUnitLength_ForAllSampledPoints()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 100);
        foreach (var s in samples)
            await Assert.That(s.Normal.Length()).IsEqualTo(1f).Within(1e-5f);
    }

    [Test]
    public async Task TangentAndNormal_ArePerpendicular_ForAllSampledPoints()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 100);
        foreach (var s in samples)
        {
            float dot = Vector2.Dot(s.Tangent, s.Normal);
            await Assert.That((double)dot).IsEqualTo(0.0).Within(1e-5);
        }
    }

    [Test]
    public async Task Normal_Is90DegCWRotationOfTangent_ExplicitlyVerified()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 50);
        foreach (var s in samples)
        {
            await Assert.That(s.Normal.X).IsEqualTo(s.Tangent.Y).Within(1e-5f);
            await Assert.That(s.Normal.Y).IsEqualTo(-s.Tangent.X).Within(1e-5f);
        }
    }

    [Test]
    public async Task SampleEvenly_PositionsFormClosedLoop_LastIsNearFirst()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 100);
        float dist = Vector2.Distance(samples[0].Position, samples[samples.Count - 1].Position);
        await Assert.That(dist).IsLessThan(3f);
    }

    [Test]
    public async Task Sample_CircleTrack_PositionsLieOnEllipse()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 100);
        var center = new Vector2(60, 25);
        const float radiusX = 28f, radiusY = 16f;
        foreach (var s in samples)
        {
            float nx = (s.Position.X - center.X) / radiusX;
            float ny = (s.Position.Y - center.Y) / radiusY;
            float r = MathF.Sqrt(nx * nx + ny * ny);
            await Assert.That((double)r).IsEqualTo(1.0).Within(0.1);
        }
    }

    [Test]
    public async Task Sample_CircleTrack_TangentAtT0_IsPerpendicularToRadius()
    {
        var s = _sampler.Sample(_circleSpline, 0.0);
        var center = new Vector2(60, 25);
        var radialDir = Vector2.Normalize(s.Position - center);
        float dot = MathF.Abs(Vector2.Dot(radialDir, s.Tangent));
        await Assert.That((double)dot).IsLessThan(0.1);
    }

    [Test]
    public async Task Sample_CircleTrack_NoLargeDiscontinuity_BetweenConsecutiveSamples()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 1000);
        for (int i = 1; i < samples.Count; i++)
        {
            float dist = Vector2.Distance(samples[i].Position, samples[i - 1].Position);
            await Assert.That(dist).IsLessThan(1f);
        }
    }

    [Test]
    public async Task Sample_SquareLoop_AtT0_IsNearFirstCorner()
    {
        var s = _sampler.Sample(_squareSpline, 0.0);
        await Assert.That(s.Position.X).IsEqualTo(0f).Within(0.1f);
        await Assert.That(s.Position.Y).IsEqualTo(0f).Within(0.1f);
    }

    [Test]
    public async Task Sample_SquareLoop_AtT025_IsNearSecondCorner()
    {
        var s = _sampler.Sample(_squareSpline, 0.25);
        await Assert.That(s.Position.X).IsEqualTo(10f).Within(0.1f);
        await Assert.That(s.Position.Y).IsEqualTo(0f).Within(0.1f);
    }

    [Test]
    public async Task Sample_SquareLoop_AtT05_IsNearThirdCorner()
    {
        var s = _sampler.Sample(_squareSpline, 0.5);
        await Assert.That(s.Position.X).IsEqualTo(10f).Within(0.1f);
        await Assert.That(s.Position.Y).IsEqualTo(10f).Within(0.1f);
    }

    [Test]
    public async Task Sample_SquareLoop_AtT075_IsNearFourthCorner()
    {
        var s = _sampler.Sample(_squareSpline, 0.75);
        await Assert.That(s.Position.X).IsEqualTo(0f).Within(0.1f);
        await Assert.That(s.Position.Y).IsEqualTo(10f).Within(0.1f);
    }

    [Test]
    public async Task Sample_SquareLoop_NormalIsAlways90DegCWFromTangent()
    {
        var samples = _sampler.SampleEvenly(_squareSpline, 20);
        foreach (var s in samples)
        {
            await Assert.That(s.Normal.X).IsEqualTo(s.Tangent.Y).Within(1e-5f);
            await Assert.That(s.Normal.Y).IsEqualTo(-s.Tangent.X).Within(1e-5f);
        }
    }

    [Test]
    public async Task Sample_TriangleLoop_PassesThroughAllThreeCorners()
    {
        var s0 = _sampler.Sample(_triangleSpline, 0.0);
        var s1 = _sampler.Sample(_triangleSpline, 1.0 / 3.0);
        var s2 = _sampler.Sample(_triangleSpline, 2.0 / 3.0);

        await Assert.That(s0.Position.X).IsEqualTo(0f).Within(0.1f);
        await Assert.That(s0.Position.Y).IsEqualTo(0f).Within(0.1f);
        await Assert.That(s1.Position.X).IsEqualTo(10f).Within(0.1f);
        await Assert.That(s1.Position.Y).IsEqualTo(0f).Within(0.1f);
        await Assert.That(s2.Position.X).IsEqualTo(5f).Within(0.1f);
        await Assert.That(s2.Position.Y).IsEqualTo(10f).Within(0.1f);
    }

    [Test]
    public async Task Sample_TriangleLoop_AllTangentsAreUnitLength()
    {
        var samples = _sampler.SampleEvenly(_triangleSpline, 30);
        foreach (var s in samples)
            await Assert.That(s.Tangent.Length()).IsEqualTo(1f).Within(1e-4f);
    }

    [Test]
    public async Task SampleEvenly_DenseSampling_AllTangentsAndNormalsValid()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 1000);
        foreach (var s in samples)
        {
            await Assert.That(s.Tangent.Length()).IsEqualTo(1f).Within(1e-4f);
            await Assert.That(s.Normal.Length()).IsEqualTo(1f).Within(1e-4f);
        }
    }

    [Test]
    public async Task SampleEvenly_SparseSampling_FourPoints_ReturnsValidSamples()
    {
        var samples = _sampler.SampleEvenly(_circleSpline, 4);
        await Assert.That(samples.Count).IsEqualTo(4);
        foreach (var s in samples)
            await Assert.That(s.Tangent.Length()).IsEqualTo(1f).Within(1e-4f);
    }

    [Test]
    public async Task Sample_DegenerateLoop_DoesNotThrow_AndFallsBackToUnitX()
    {
        var degenerate = new SplineLoop(new[]
        {
            new Vector2(5, 5), new Vector2(5, 5), new Vector2(5, 5), new Vector2(5, 5)
        });

        await Assert.That(() => _sampler.Sample(degenerate, 0.5)).ThrowsNothing();

        var s = _sampler.Sample(degenerate, 0.0);
        await Assert.That(s.Tangent.Length()).IsEqualTo(1f).Within(1e-5f);
        await Assert.That(s.Tangent.X).IsEqualTo(1f).Within(1e-5f);
        await Assert.That(s.Tangent.Y).IsEqualTo(0f).Within(1e-5f);
    }

    [Test]
    public async Task Sample_OvalTrack_PositionsWithinExpectedBounds()
    {
        var ovalSpec = new OvalTrack().BuildSpec;
        var samples = _sampler.SampleEvenly(ovalSpec.CenterLine, 100);
        foreach (var s in samples)
        {
            await Assert.That(s.Position.X).IsBetween(60 - 36 - 1, 60 + 36 + 1);
            await Assert.That(s.Position.Y).IsBetween(25 - 14 - 1, 25 + 14 + 1);
        }
    }

    [Test]
    [Arguments(10)]
    [Arguments(50)]
    [Arguments(200)]
    public async Task SampleEvenly_VariousCounts_AlwaysReturnsRequestedCount(int count)
    {
        await Assert.That(_sampler.SampleEvenly(_circleSpline, count).Count).IsEqualTo(count);
    }
}
