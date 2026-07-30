namespace Novolis.Simulation.Mesh.Tests;

using TUnit.Assertions;

public sealed class MeshPathfinderTests
{
    [Test]
    public async Task FindPath_Triangle_SolToWolf_IsDirect()
    {
        var state = MeshTestGraph.Triangle();
        var path = MeshPathfinder.FindPath(state, MeshTestGraph.Sol, MeshTestGraph.Wolf);

        await Assert.That(path).IsNotNull();
        await Assert.That(path!.Value.Length).IsEqualTo(2);
        await Assert.That(path.Value[0]).IsEqualTo(MeshTestGraph.Sol);
        await Assert.That(path.Value[1]).IsEqualTo(MeshTestGraph.Wolf);
    }

    [Test]
    public async Task Empty_HasZeroHourIndex()
    {
        var state = MeshState.Empty();
        await Assert.That(state.HourIndex).IsEqualTo(0);
        await Assert.That(state.Nodes.Count).IsEqualTo(0);
    }
}
