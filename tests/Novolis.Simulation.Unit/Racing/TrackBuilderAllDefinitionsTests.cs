namespace Novolis.Simulation.Racing.Tests;

using System.Numerics;

using Novolis.Simulation.Racing.Tracks;

using TUnit.Assertions;

public sealed class TrackBuilderAllDefinitionsTests
{
    public static IEnumerable<Func<ITrackDefinition>> AllDefinitions() =>
        BuiltInTracks.All.Select<ITrackDefinition, Func<ITrackDefinition>>(t => () => t);

    [Test]
    [MethodDataSource(nameof(AllDefinitions))]
    public async Task Build_AllBuiltInDefinitions_ProducesValidTrack(ITrackDefinition definition)
    {
        var track = new TrackBuilder().Build(definition);
        await Assert.That(track.Width).IsGreaterThan(0);
        await Assert.That(track.Height).IsGreaterThan(0);
        await Assert.That(track.Gates.Count).IsGreaterThan(0);
        await Assert.That(track.Cells).IsNotNull();
        await Assert.That(track.Cells.GetLength(0)).IsEqualTo(track.Width);
        await Assert.That(track.Cells.GetLength(1)).IsEqualTo(track.Height);
    }

    [Test]
    [MethodDataSource(nameof(AllDefinitions))]
    public async Task Build_AllDefinitions_StartPoseForwardIsUnit(ITrackDefinition definition)
    {
        var track = new TrackBuilder().Build(definition);
        var f = track.StartPose.Forward;
        var len = MathF.Sqrt(f.X * f.X + f.Y * f.Y);
        await Assert.That(len).IsEqualTo(1f).Within(1e-4f);
    }
}
