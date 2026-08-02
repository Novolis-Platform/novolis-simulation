using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh.Tests;

public sealed class MeshPipelineTests
{
    [Test]
    public async Task PublishPulse_Feed_CreditsOriginNode()
    {
        var state = MeshTestGraph.Triangle();
        var (next, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsGeneral),
            subject: "spot digest");

        await Assert.That(next.IsVisibleAt(id, MeshTestGraph.Sol)).IsTrue();
        await Assert.That(next.Stats.FeedPublishes).IsEqualTo(1);
    }

    [Test]
    public async Task PublishPulse_Identity_PushesCoLocatedMailbox()
    {
        var owner = MeshIdentityIds.Firm("acme");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        var (next, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToIdentity(owner),
            subject: "invoice");

        await Assert.That(next.IsInMailbox(id, owner)).IsTrue();
        await Assert.That(next.Stats.IdentityPublishes).IsEqualTo(1);
        await Assert.That(next.Stats.MailboxPushes).IsGreaterThan(0);
    }

    [Test]
    public async Task PublishDirected_EnqueuesLaunch_AndDeliversAfterTransit()
    {
        var state = MeshTestGraph.Triangle();
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToPlace(MeshTestGraph.Wolf));

        await Assert.That(published.Pending.Length).IsEqualTo(1);
        await Assert.That(published.Stats.DirectedPublishes).IsEqualTo(1);

        var advanced = published;
        for (var i = 0; i < 4 && !advanced.IsVisibleAt(id, MeshTestGraph.Wolf); i++)
        {
            advanced = DefaultMeshPipeline.Advance(advanced);
        }

        await Assert.That(advanced.IsVisibleAt(id, MeshTestGraph.Wolf)).IsTrue();
        await Assert.That(advanced.Stats.DronesLaunched).IsGreaterThan(0);
        await Assert.That(advanced.Stats.DronesArrived).IsGreaterThan(0);
        InvariantChecker.AssertAll(advanced);
    }

    [Test]
    public async Task FeedEngine_SubscribePull_PutsFeedPacketInInbox()
    {
        var owner = MeshIdentityIds.Person("pilot");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        state = FeedEngine.Subscribe(state, owner, MeshFeedId.CommerceSpot);
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.CommerceSpot),
            topic: MeshTopics.SpotDigest);

        var pulled = FeedEngine.Pull(published, owner);
        await Assert.That(pulled.IsInFeedInbox(id, owner)).IsTrue();
        await Assert.That(pulled.Stats.FeedPulls).IsEqualTo(1);
    }

    [Test]
    public async Task FeedEngine_CannotUnsubscribeEmergency()
    {
        var owner = MeshIdentityIds.Household("h1");
        var state = FeedEngine.Subscribe(MeshTestGraph.Triangle(), owner, MeshFeedId.Emergency);
        var after = FeedEngine.Unsubscribe(state, owner, MeshFeedId.Emergency);
        await Assert.That(FeedEngine.EffectiveFeedIds(after, owner).Contains(MeshFeedId.Emergency.Value)).IsTrue();
    }

    [Test]
    public async Task PublishRetraction_MarksLogicalKeyAtNode()
    {
        var state = MeshTestGraph.Triangle();
        var (next, _) = PublishEngine.PublishRetraction(
            state,
            MeshTestGraph.Sol,
            logicalKey: "job-42");

        await Assert.That(next.IsRetractedAt("job-42", MeshTestGraph.Sol)).IsTrue();
        await Assert.That(next.Stats.RetractionsApplied).IsEqualTo(1);
    }

    [Test]
    public async Task TtlEngine_GlobalExpiry_RemovesPacketEverywhere()
    {
        var state = MeshTestGraph.Triangle();
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsPrices),
            globalTtlHours: 1);

        var expired = published;
        for (var i = 0; i < 3; i++)
        {
            expired = DefaultMeshPipeline.Advance(expired);
        }

        await Assert.That(expired.TryGetPacket(id, out _)).IsFalse();
        await Assert.That(expired.Stats.GlobalPacketDrops).IsGreaterThan(0);
    }

    [Test]
    public async Task MeshVisibility_EnforcesNodeCacheCap()
    {
        var policy = new MeshPolicy(MaxPacketsPerNodeCache: 2);
        var state = MeshTestGraph.Triangle(policy);

        for (var i = 0; i < 3; i++)
        {
            (state, _) = PublishEngine.PublishPulse(
                state,
                MeshTestGraph.Sol,
                MeshAddress.ToFeed(MeshFeedId.NewsGeneral),
                subject: $"msg-{i}");
        }

        await Assert.That(state.NodeCaches[MeshTestGraph.Sol.Value].Count).IsLessThanOrEqualTo(2);
        await Assert.That(state.Stats.LocalCacheDrops).IsGreaterThan(0);
    }

    [Test]
    public async Task FloodEngine_SpreadsIdentityPacketToNeighbor()
    {
        var owner = MeshIdentityIds.Ship("tramp");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToIdentity(owner));

        var flooded = FloodEngine.Dispatch(published);
        flooded = LaunchEngine.LaunchPending(flooded);
        await Assert.That(flooded.Pending.Length + flooded.Drones.Length).IsGreaterThan(0);
        await Assert.That(flooded.IsVisibleAt(id, MeshTestGraph.Sol)).IsTrue();
    }

    [Test]
    public async Task MailboxEngine_Move_CatchesUpMandatoryEmergency()
    {
        var owner = MeshIdentityIds.Firm("mover");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        var (withAlert, alertId) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Wolf,
            MeshAddress.ToFeed(MeshFeedId.Emergency),
            topic: MeshTopics.Emergency);

        for (var i = 0; i < 5 && !withAlert.IsVisibleAt(alertId, MeshTestGraph.Wolf); i++)
        {
            withAlert = DefaultMeshPipeline.Advance(withAlert);
        }

        var moved = MailboxEngine.Move(withAlert, owner, MeshTestGraph.Wolf);
        await Assert.That(moved.IsInFeedInbox(alertId, owner)).IsTrue();
    }

    [Test]
    public async Task InvariantChecker_FlagsBrokenDroneReference()
    {
        var state = MeshTestGraph.Triangle() with
        {
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(),
                    PacketId.New(),
                    MeshTestGraph.Sol,
                    MeshTestGraph.Wolf,
                    1,
                    ImmutableArray<MeshNodeId>.Empty,
                    false,
                    1),
            ],
        };

        var errors = InvariantChecker.Check(state);
        await Assert.That(errors.Count).IsGreaterThan(0);
        await Assert.That(errors[0]).Contains("missing packet");
    }

    [Test]
    public async Task MeshEngine_StepsRunInOrder()
    {
        var engine = DefaultMeshPipeline.CreateEngine();
        await Assert.That(engine.Steps.Count).IsEqualTo(6);
        await Assert.That(engine.Steps[0].Name).IsEqualTo("DroneTick");
        await Assert.That(engine.Steps[^1].Name).IsEqualTo("TtlExpire");
    }

    [Test]
    public async Task Publish_DuplicatePacket_Throws()
    {
        var state = MeshTestGraph.Triangle();
        var id = PacketId.New();
        var packet = new MeshPacket(
            id,
            MeshTrafficLayer.Feed,
            true,
            ImmutableArray<byte>.Empty,
            1,
            null,
            null,
            1,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsGeneral),
            0);

        state = PublishEngine.Publish(state, packet, MeshTestGraph.Sol);
        await Assert.That(() => PublishEngine.Publish(state, packet, MeshTestGraph.Sol)).Throws<InvalidOperationException>();
    }
}
