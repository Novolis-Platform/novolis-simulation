using System.Numerics;

using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Tracks;
using Novolis.Simulation.Racing.Tests.Infrastructure;

using TUnit.Assertions;

namespace Novolis.Simulation.Racing.Tests;

public class TrackProgressResolverTests : BaseTest
{
    private static readonly TrackBuilder _builder = new();
    private static readonly TrackProgressResolver _resolver = new();
    private static readonly RaceTrack _circle = _builder.Build(BuiltInTracks.Circle);

    private static readonly Vector2 _startPos = _circle.ProgressMap.Samples[0];
    private static readonly Vector2 _startTangent = _circle.ProgressMap.Tangents[0];
    private static readonly Vector2 _startNormal = new(_startTangent.Y, -_startTangent.X);

    private static readonly Vector2 _midPos = _circle.ProgressMap.Samples[500];
    private static readonly Vector2 _midTangent = _circle.ProgressMap.Tangents[500];

    [Test]
    public async Task Resolve_LoopT_IsAlwaysAtLeastZero()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startTangent);
        await Assert.That(result.LoopT).IsGreaterThanOrEqualTo(0.0);
    }

    [Test]
    public async Task Resolve_LoopT_IsAlwaysLessThanOne()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startTangent);
        await Assert.That(result.LoopT).IsLessThan(1.0);
    }

    [Test]
    public async Task Resolve_StartPosition_ReturnsLoopTNearZero()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startTangent);
        await Assert.That(result.LoopT).IsEqualTo(0.0).Within(0.01);
    }

    [Test]
    public async Task Resolve_OppositePosition_ReturnsLoopTNearHalf()
    {
        var result = _resolver.Resolve(_circle, _midPos, _midTangent);
        await Assert.That(result.LoopT).IsEqualTo(0.5).Within(0.05);
    }

    [Test]
    public async Task Resolve_LoopT_IsInRangeZeroToOne_ForMultiplePositions()
    {
        int step = _circle.ProgressMap.Samples.Count / 10;
        for (int i = 0; i < 10; i++)
        {
            var pos = _circle.ProgressMap.Samples[i * step];
            var tan = _circle.ProgressMap.Tangents[i * step];
            var result = _resolver.Resolve(_circle, pos, tan);
            await Assert.That(result.LoopT).IsBetween(0.0, 1.0);
        }
    }

    [Test]
    public async Task Resolve_TotalProgress_EqualsLoopT()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startTangent);
        await Assert.That(result.TotalProgress).IsEqualTo(result.LoopT);
    }

    [Test]
    public async Task Resolve_TotalProgress_EqualsLoopT_AtMidpoint()
    {
        var result = _resolver.Resolve(_circle, _midPos, _midTangent);
        await Assert.That(result.TotalProgress).IsEqualTo(result.LoopT);
    }

    [Test]
    public async Task Resolve_Alignment_IsAlwaysInRangeZeroToOne()
    {
        int step = _circle.ProgressMap.Samples.Count / 10;
        for (int i = 0; i < 10; i++)
        {
            var pos = _circle.ProgressMap.Samples[i * step];
            var tan = _circle.ProgressMap.Tangents[i * step];
            var result = _resolver.Resolve(_circle, pos, tan);
            await Assert.That(result.Alignment).IsBetween(0.0, 1.0);
        }
    }

    [Test]
    public async Task Resolve_ForwardAlignedWithTangent_AlignmentNearOne()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startTangent);
        await Assert.That(result.Alignment).IsEqualTo(1.0).Within(0.01);
    }

    [Test]
    public async Task Resolve_ForwardOppositeToTangent_AlignmentNearZero()
    {
        var opposite = -_startTangent;
        var result = _resolver.Resolve(_circle, _startPos, opposite);
        await Assert.That(result.Alignment).IsEqualTo(0.0).Within(0.01);
    }

    [Test]
    public async Task Resolve_ForwardPerpendicular_AlignmentNearHalf()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startNormal);
        await Assert.That(result.Alignment).IsEqualTo(0.5).Within(0.01);
    }

    [Test]
    public async Task Resolve_ForwardAlignedWithTangent_IsNotWrongWay()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startTangent);
        await Assert.That(result.IsWrongWay).IsFalse();
    }

    [Test]
    public async Task Resolve_ForwardOppositeToTangent_IsWrongWay()
    {
        var result = _resolver.Resolve(_circle, _startPos, -_startTangent);
        await Assert.That(result.IsWrongWay).IsTrue();
    }

    [Test]
    public async Task Resolve_ForwardPerpendicular_IsNotWrongWay()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startNormal);
        await Assert.That(result.IsWrongWay).IsFalse();
    }

    [Test]
    public async Task Resolve_ForwardJustBelowWrongWayThreshold_IsWrongWay()
    {
        var backComponent = -_startTangent * 0.95f;
        var sideComponent = _startNormal * 0.31f;
        var forwardJustWrong = Vector2.Normalize(backComponent + sideComponent);
        var result = _resolver.Resolve(_circle, _startPos, forwardJustWrong);
        await Assert.That(result.IsWrongWay).IsTrue();
    }

    [Test]
    public async Task Resolve_PositionExactlyOnCenterline_OffsetIsNearZero()
    {
        var result = _resolver.Resolve(_circle, _startPos, _startTangent);
        await Assert.That(result.SignedCenterOffset).IsEqualTo(0.0).Within(1e-4);
    }

    [Test]
    public async Task Resolve_PositionOffsetRight_ProducesPositiveOffset()
    {
        var rightPos = _startPos + _startNormal * 1.5f;
        var result = _resolver.Resolve(_circle, rightPos, _startTangent);
        await Assert.That(result.SignedCenterOffset).IsGreaterThan(0.0);
    }

    [Test]
    public async Task Resolve_PositionOffsetLeft_ProducesNegativeOffset()
    {
        var leftPos = _startPos - _startNormal * 1.5f;
        var result = _resolver.Resolve(_circle, leftPos, _startTangent);
        await Assert.That(result.SignedCenterOffset).IsLessThan(0.0);
    }

    [Test]
    public async Task Resolve_OffsetMagnitude_IsApproximatelyTheDisplacement()
    {
        float displacement = 1.5f;
        var rightPos = _startPos + _startNormal * displacement;
        var result = _resolver.Resolve(_circle, rightPos, _startTangent);
        await Assert.That(result.SignedCenterOffset).IsEqualTo(displacement).Within(0.1);
    }

    [Test]
    public async Task Resolve_TenDifferentPositions_AllReturnValidLoopT()
    {
        int step = _circle.ProgressMap.Samples.Count / 10;
        for (int i = 0; i < 10; i++)
        {
            var pos = _circle.ProgressMap.Samples[i * step];
            var tan = _circle.ProgressMap.Tangents[i * step];
            var result = _resolver.Resolve(_circle, pos, tan);
            await Assert.That(result.LoopT).IsBetween(0.0, 1.0)
                .Because($"sample {i * step} should return valid LoopT");
        }
    }

    [Test]
    public async Task Resolve_TenDifferentPositions_AllReturnValidAlignment()
    {
        int step = _circle.ProgressMap.Samples.Count / 10;
        for (int i = 0; i < 10; i++)
        {
            var pos = _circle.ProgressMap.Samples[i * step];
            var tan = _circle.ProgressMap.Tangents[i * step];
            var result = _resolver.Resolve(_circle, pos, tan);
            await Assert.That(result.Alignment).IsBetween(0.0, 1.0)
                .Because($"sample {i * step} should return valid Alignment");
        }
    }

    [Test]
    public async Task Resolve_EachProgressMapSample_GivesCorrectLoopT()
    {
        var map = _circle.ProgressMap;
        int[] indices = [0, 250, 500, 750];
        foreach (int idx in indices)
        {
            var expectedLoopT = map.CumulativeArcLengths[idx] / map.TotalArcLength;
            var result = _resolver.Resolve(_circle, map.Samples[idx], map.Tangents[idx]);
            await Assert.That(result.LoopT).IsEqualTo(expectedLoopT).Within(0.001)
                .Because($"sample at index {idx} should resolve to its stored arc-length fraction");
        }
    }
}
