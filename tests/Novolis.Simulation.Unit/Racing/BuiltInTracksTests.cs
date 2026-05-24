namespace Novolis.Simulation.Racing.Tests;

using Novolis.Simulation.Racing.Tracks;

using TUnit.Assertions;

public sealed class BuiltInTracksTests
{
    [Test]
    public async Task All_ContainsDistinctIds()
    {
        var ids = BuiltInTracks.All.Select(t => t.Id).ToList();
        await Assert.That(ids.Distinct().Count()).IsEqualTo(ids.Count);
        await Assert.That(ids.Count).IsGreaterThan(0);
    }

    [Test]
    [Arguments("circle")]
    [Arguments("CIRCLE")]
    public async Task ById_Circle_IsCaseInsensitive(string id) =>
        await Assert.That(BuiltInTracks.ById(id)).IsSameReferenceAs(BuiltInTracks.Circle);

    [Test]
    [Arguments("micro-circle")]
    [Arguments("Micro-Circle")]
    public async Task ById_MicroCircle_IsCaseInsensitive(string id) =>
        await Assert.That(BuiltInTracks.ById(id)).IsSameReferenceAs(BuiltInTracks.MicroCircle);

    [Test]
    public async Task ById_Unknown_ReturnsCircleFallback()
    {
        var t = BuiltInTracks.ById("no-such-track-xyz");
        await Assert.That(t).IsSameReferenceAs(BuiltInTracks.Circle);
    }

    [Test]
    public async Task Reasonable_IsSubsetOfAll()
    {
        var allIds = BuiltInTracks.All.Select(t => t.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var t in BuiltInTracks.Reasonable)
            await Assert.That(allIds.Contains(t.Id)).IsTrue();
    }
}
