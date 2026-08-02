namespace Novolis.Simulation.Replay.Tests;

using Novolis.Simulation.Replay;

public sealed class ReplayBufferTests
{
    [Test]
    public async Task EventStore_AppendsAndReadsAll()
    {
        var store = new InMemorySimulationEventStore<string>();
        await store.AppendAsync("alpha");
        await store.AppendAsync("beta");

        var read = new List<string>();
        await foreach (var evt in store.ReadAllAsync())
        {
            read.Add(evt);
        }

        await Assert.That(read).IsEquivalentTo(["alpha", "beta"]);
    }

    [Test]
    public async Task PlanBuffer_OrdersByActorId()
    {
        var buffer = new SimultaneousPlanBuffer<string>();
        buffer.Submit(3, "c");
        buffer.Submit(1, "a");
        buffer.Submit(2, "b");

        var pending = buffer.PendingPlans();
        await Assert.That(pending[0].Key).IsEqualTo(1);
        await Assert.That(pending[1].Key).IsEqualTo(2);
        await Assert.That(pending[2].Key).IsEqualTo(3);

        buffer.Clear();
        await Assert.That(buffer.PendingPlans().Count).IsEqualTo(0);
    }
}
