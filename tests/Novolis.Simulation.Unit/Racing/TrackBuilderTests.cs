using System.Numerics;

using Novolis.Simulation.Racing.Tracks;
using Novolis.Simulation.Racing.Tests.Infrastructure;

using TUnit.Assertions;

namespace Novolis.Simulation.Racing.Tests;

public class TrackBuilderTests : BaseTest
{
    private static readonly TrackBuilder _builder = new();
    private static readonly RaceTrack _circle = _builder.Build(BuiltInTracks.Circle);

    [Test]
    public async Task Build_CircleTrack_WidthMatchesSpec()
    {
        await Assert.That(_circle.Width).IsEqualTo(120);
    }

    [Test]
    public async Task Build_CircleTrack_HeightMatchesSpec()
    {
        await Assert.That(_circle.Height).IsEqualTo(50);
    }

    [Test]
    public async Task Build_CircleTrack_CellsArrayIsNonNull()
    {
        await Assert.That(_circle.Cells).IsNotNull();
    }

    [Test]
    public async Task Build_CircleTrack_HasRoadCells()
    {
        await Assert.That(CountCells(_circle, TrackCell.Road)).IsGreaterThan(0);
    }

    [Test]
    public async Task Build_CircleTrack_HasWallCells()
    {
        await Assert.That(CountCells(_circle, TrackCell.Wall)).IsGreaterThan(0);
    }

    [Test]
    public async Task Build_CircleTrack_HasEmptyCells()
    {
        await Assert.That(CountCells(_circle, TrackCell.Empty)).IsGreaterThan(0);
    }

    [Test]
    public async Task Build_CircleTrack_HasGateCells()
    {
        await Assert.That(CountCells(_circle, TrackCell.Gate)).IsGreaterThan(0);
    }

    [Test]
    public async Task Build_CircleTrack_HasExactlyOneStartFinishCell()
    {
        await Assert.That(CountCells(_circle, TrackCell.StartFinish)).IsEqualTo(1);
    }

    [Test]
    public async Task Build_CircleTrack_GateCountMatchesSpec()
    {
        await Assert.That(_circle.Gates.Count).IsEqualTo(12);
    }

    [Test]
    public async Task Build_CircleTrack_CellsAtEdges_AreAccessible()
    {
        await Assert.That(() =>
        {
            _ = _circle.Cells[0, 0];
            _ = _circle.Cells[_circle.Width - 1, _circle.Height - 1];
            _ = _circle.Cells[_circle.Width - 1, 0];
            _ = _circle.Cells[0, _circle.Height - 1];
        }).ThrowsNothing();
    }

    [Test]
    public async Task Build_CircleTrack_StartPosePositionIsWithinTrackBounds()
    {
        var pos = _circle.StartPose.Position;
        await Assert.That(pos.X).IsBetween(0, _circle.Width);
        await Assert.That(pos.Y).IsBetween(0, _circle.Height);
    }

    [Test]
    public async Task Build_CircleTrack_StartPoseForwardIsUnitVector()
    {
        var fwd = _circle.StartPose.Forward;
        await Assert.That((double)fwd.Length()).IsEqualTo(1.0).Within(1e-5);
    }

    [Test]
    public async Task Build_CircleTrack_CenterLineSamplesCountIs1000()
    {
        await Assert.That(_circle.CenterLineSamples.Count).IsEqualTo(1000);
    }

    [Test]
    public async Task Build_CircleTrack_ProgressMap_FirstArcLengthIsZero()
    {
        await Assert.That(_circle.ProgressMap.CumulativeArcLengths[0]).IsEqualTo(0.0);
    }

    [Test]
    public async Task Build_CircleTrack_ProgressMap_IsMonotonicallyIncreasing()
    {
        var arcLens = _circle.ProgressMap.CumulativeArcLengths;
        for (int i = 1; i < arcLens.Count; i++)
            await Assert.That(arcLens[i]).IsGreaterThanOrEqualTo(arcLens[i - 1]);
    }

    [Test]
    public async Task Build_CircleTrack_ProgressMap_TotalArcLengthIsPositive()
    {
        await Assert.That(_circle.ProgressMap.TotalArcLength).IsGreaterThan(0.0);
    }

    [Test]
    public async Task Build_CircleTrack_ProgressMap_TotalArcLengthExceedsLastCumulativeSample()
    {
        var map = _circle.ProgressMap;
        await Assert.That(map.TotalArcLength).IsGreaterThan(map.CumulativeArcLengths[map.CumulativeArcLengths.Count - 1]);
    }

    [Test]
    public async Task Build_CircleTrack_ProgressMap_SamplesCountIs1000()
    {
        await Assert.That(_circle.ProgressMap.Samples.Count).IsEqualTo(1000);
    }

    [Test]
    public async Task Build_CircleTrack_ProgressMap_TangentsCountIs1000()
    {
        await Assert.That(_circle.ProgressMap.Tangents.Count).IsEqualTo(1000);
    }

    [Test]
    public async Task Build_CircleTrack_Geometry_HalfWidthMatchesSpec()
    {
        await Assert.That(_circle.Geometry.HalfWidth).IsEqualTo(4.5).Within(1e-10);
    }

    [Test]
    public async Task Build_CircleTrack_Geometry_LeftBoundaryCountIs1000()
    {
        await Assert.That(_circle.Geometry.LeftBoundary.Count).IsEqualTo(1000);
    }

    [Test]
    public async Task Build_CircleTrack_Geometry_RightBoundaryCountIs1000()
    {
        await Assert.That(_circle.Geometry.RightBoundary.Count).IsEqualTo(1000);
    }

    [Test]
    public async Task Build_CircleTrack_HasExpectedId()
    {
        await Assert.That(_circle.Id).IsEqualTo("circle");
    }

    [Test]
    [MethodDataSource(nameof(AllBuiltInTrackDefinitions))]
    public async Task Build_AllBuiltInTracks_DoNotThrow(ITrackDefinition definition)
    {
        await Assert.That(() => _builder.Build(definition)).ThrowsNothing();
    }

    [Test]
    [MethodDataSource(nameof(AllBuiltInTrackDefinitions))]
    public async Task Build_AllBuiltInTracks_ProduceCenterLineSamplesOf1000(ITrackDefinition definition)
    {
        var track = _builder.Build(definition);
        await Assert.That(track.CenterLineSamples.Count).IsEqualTo(1000);
    }

    public static IEnumerable<Func<ITrackDefinition>> AllBuiltInTrackDefinitions() =>
        BuiltInTracks.All.Select<ITrackDefinition, Func<ITrackDefinition>>(t => () => t);

    [Test]
    public async Task Build_SmallCircle_CellsAreWithinGridBounds()
    {
        var spec = TrackSpecs.Circle("small", 40, 40, new Vector2(20, 20), 10f, 10f, 3.0, 8, 4, 1);
        var def = new SimpleTrackDefinition("small-circle", "Small Circle", spec);
        var track = _builder.Build(def);

        await Assert.That(track.Width).IsEqualTo(40);
        await Assert.That(track.Height).IsEqualTo(40);
        await Assert.That(track.Cells.GetLength(0)).IsEqualTo(40);
        await Assert.That(track.Cells.GetLength(1)).IsEqualTo(40);
    }

    [Test]
    public async Task Build_NarrowTrack_StillProducesRoadCells()
    {
        var spec = TrackSpecs.Circle("narrow", 60, 60, new Vector2(30, 30), 20f, 20f, 1.0, 8, 4, 1);
        var def = new SimpleTrackDefinition("narrow", "Narrow", spec);
        var track = _builder.Build(def);
        await Assert.That(CountCells(track, TrackCell.Road)).IsGreaterThan(0);
    }

    [Test]
    [Arguments(4)]
    [Arguments(8)]
    [Arguments(16)]
    public async Task Build_CustomGateCount_MatchesExpected(int gateCount)
    {
        var spec = TrackSpecs.Circle("g", 120, 50, new Vector2(60, 25), 28f, 16f, 4.5, 16, gateCount, 1);
        var def = new SimpleTrackDefinition($"g{gateCount}", "GateTest", spec);
        var track = _builder.Build(def);
        await Assert.That(track.Gates.Count).IsEqualTo(gateCount);
    }

    [Test]
    public async Task Build_CircleTrack_ProgressMap_ArcLengthsCountIs1000()
    {
        await Assert.That(_circle.ProgressMap.CumulativeArcLengths.Count).IsEqualTo(1000);
    }

    private static int CountCells(RaceTrack track, TrackCell type)
    {
        int count = 0;
        for (int c = 0; c < track.Width; c++)
            for (int r = 0; r < track.Height; r++)
                if (track.Cells[c, r] == type)
                    count++;
        return count;
    }

    private sealed class SimpleTrackDefinition(string id, string name, TrackBuildSpec spec) : ITrackDefinition
    {
        public string Id => id;
        public string Name => name;
        public TrackBuildSpec BuildSpec => spec;
    }
}
