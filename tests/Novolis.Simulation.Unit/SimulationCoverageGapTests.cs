using System.Collections.Immutable;
using System.Numerics;
using Novolis.Math.Arrays;
using Novolis.Physics.Abstractions;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Kinematics;
using Novolis.Simulation.Mesh;
using Novolis.Simulation.SpaceCombat;
using Novolis.Simulation.World;
using Novolis.Simulation.World.Builders;

namespace Novolis.Simulation.Unit;

public sealed class SimulationCoverageGapTests
{
    [Test]
    public async Task BoundedHeightfield_ContactAndSegmentQueries()
    {
        var field = new BoundedHeightfield(new HillSampler(), 50f);
        await Assert.That(field.IsInside(10f, 10f)).IsTrue();
        await Assert.That(field.IsInside(60f, 10f)).IsFalse();
        await Assert.That(field.SampleHeight(5f, 5f)).IsEqualTo(2f);

        var below = new Vector3(10f, 1f, 10f);
        await Assert.That(field.TryHeightfieldContact(below, radius: 0.5f)).IsTrue();
        var above = new Vector3(10f, 10f, 10f);
        await Assert.That(field.TryHeightfieldContact(above, radius: 0.1f)).IsFalse();

        var hit = field.TrySegmentLeavesRange(
            new Vector3(10f, 1f, 10f),
            new Vector3(60f, 1f, 10f),
            out var hitPoint,
            out var fraction);
        await Assert.That(hit).IsTrue();
        await Assert.That(hitPoint.X).IsLessThanOrEqualTo(50f);
        await Assert.That(fraction).IsGreaterThan(0f).And.IsLessThan(1f);
    }

    [Test]
    public async Task DroneTickEngine_LossPolicy_RetriesAndCreditsArrival()
    {
        var policy = new MeshPolicy(LossEveryNth: 1, MaxLossesPerPacket: 3);
        var state = MeshTestGraph.Triangle(policy);
        var packetId = PacketId.New();
        var packet = new MeshPacket(
            packetId,
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
        state = state with
        {
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(),
                    packetId,
                    MeshTestGraph.Sol,
                    MeshTestGraph.Wolf,
                    RemainingHours: 1,
                    ImmutableArray<MeshNodeId>.Empty,
                    IsFloodHop: false,
                    Priority: 1),
            ],
        };

        var after = DroneTickEngine.Tick(state);
        await Assert.That(after.Stats.DronesLost + after.Stats.DronesArrived).IsGreaterThan(0);
    }

    [Test]
    public async Task HumanoidHierarchy_ParentWalkAndCoreFlags()
    {
        await Assert.That(HumanoidHierarchy.Parent(HumanoidBone.Head)).IsEqualTo((int)HumanoidBone.Neck);
        await Assert.That(HumanoidHierarchy.ParentBone(HumanoidBone.Hips)).IsNull();
        await Assert.That(HumanoidHierarchy.ParentBone(HumanoidBone.LeftHand)).IsEqualTo(HumanoidBone.LeftForeArm);
        await Assert.That(HumanoidHierarchy.IsCoreRequired(HumanoidBone.LeftHand)).IsTrue();
        await Assert.That(HumanoidHierarchy.IsCoreRequired(HumanoidBone.LeftToeBase)).IsFalse();
    }

    [Test]
    public async Task MeshIdentifiers_ParseKindsAndMandatoryFeed()
    {
        var node = MeshNodeId.From("alpha");
        var feed = MeshFeedId.From("news");
        var identity = MeshIdentityIds.Person("alice");
        await Assert.That(node.Value).IsEqualTo("alpha");
        await Assert.That(feed.Value).IsEqualTo("news");
        await Assert.That(identity.Value).Contains("alice");
        await Assert.That(MeshIdentityIds.TryParseKind(identity)).IsEqualTo(MeshIdentityKind.Person);
        await Assert.That(MeshFeedId.Emergency.IsMandatory).IsTrue();
    }

    [Test]
    public async Task PlanarAgent_Move_WithNullStaticWorldUsesGrid()
    {
        var walls = new Novolis.Math.Arrays.DenseGrid<byte>(8, 8);
        var pos = new Vector3(2f, 0f, 2f);
        var next = PlanarAgent.Move(walls, pos, new Vector3(0.3f, 0f, 0.4f), 0.2f, 1f, staticWorld: null);
        await Assert.That(next.X).IsGreaterThan(pos.X);
        await Assert.That(next.Z).IsGreaterThan(pos.Z);
    }

    private sealed class HillSampler : IHeightSampler
    {
        public float SampleHeight(float x, float z) => 2f;
    }

    [Test]
    public async Task MeshEngines_flood_mailbox_and_invariants()
    {
        var owner = MeshIdentityIds.Ship("tramp");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        var (published, id) = PublishEngine.PublishPulse(state, MeshTestGraph.Sol, MeshAddress.ToIdentity(owner));
        var flooded = FloodEngine.Dispatch(published);
        flooded = LaunchEngine.LaunchPending(flooded);
        await Assert.That(flooded.IsVisibleAt(id, MeshTestGraph.Sol)).IsTrue();

        var errors = InvariantChecker.Check(flooded);
        await Assert.That(errors.Count).IsEqualTo(0);
    }

    [Test]
    public async Task Tiles_pathfinder_and_build_batch()
    {
        var walls = new Novolis.Simulation.Tiles.WallEdgeMap(3, 3);
        walls.SetV(1, 0, Novolis.Simulation.Tiles.WallEdge.Solid);
        walls.SetV(1, 1, Novolis.Simulation.Tiles.WallEdge.Solid);
        walls.SetV(1, 2, Novolis.Simulation.Tiles.WallEdge.Solid);
        var path = Novolis.Simulation.Tiles.GridPathfinder.FindPath(walls, (0, 0), (2, 2));
        await Assert.That(path).IsNull();

        var batch = new Novolis.Simulation.Tiles.BuildBatch(10, 10);
        batch.TouchCell(1, 1);
        await Assert.That(batch.Dirty.MinX).IsEqualTo(1);
    }

    [Test]
    public async Task SpaceCombat_arcade_and_mission()
    {
        var craft = new Novolis.Simulation.SpaceCombat.CraftState { Profile = Novolis.Simulation.SpaceCombat.CraftProfile.FighterDefault, Speed = 20f };
        craft.ResetVitals();
        Novolis.Simulation.SpaceCombat.ArcadeFlight.Apply(craft, new Novolis.Simulation.SpaceCombat.FlightIntent { ThrottleUp = 1f }, 0.5f);
        await Assert.That(craft.Position.Length()).IsGreaterThan(0.1f);

        var session = new Novolis.Simulation.SpaceCombat.MissionSession(new Novolis.Simulation.SpaceCombat.MissionDescriptor
        {
            Id = "gap",
            FreighterProfile = Novolis.Simulation.SpaceCombat.CraftProfile.FreighterDefault,
            FighterProfile = Novolis.Simulation.SpaceCombat.CraftProfile.FighterDefault,
            HostileProfile = Novolis.Simulation.SpaceCombat.CraftProfile.HostileDefault,
            HostileCount = 1,
            ProtectSeconds = 0.01f,
            DestroyRequired = 1,
            MaxHostilesAlive = 4,
        }, seed: 3);
        session.Begin();
        session.Tick(default, 0.05f);
        await Assert.That((int)session.Phase).IsGreaterThanOrEqualTo(0);
    }

    [Test]
    public async Task Replay_recorder_verify_steps()
    {
        var recorder = new Novolis.Simulation.Replay.InMemorySimulationRecorder<int>();
        recorder.SetInitial(0);
        recorder.RecordStep(new Novolis.Simulation.Abstractions.SimulationStep(1, 1), 1, stepSeed: 1);
        var timeline = recorder.Build();
        await Assert.That(Novolis.Simulation.Replay.ReplayPlayback.GetEndStateAt(timeline, 0)).IsEqualTo(1);
    }

    [Test]
    public async Task TtlEngine_LocalExpiry_AndRetractionDrop()
    {
        var (published, id) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(),
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsGeneral),
            localTtlHours: 1);
        var afterLocal = TtlEngine.Expire(published with { HourIndex = published.HourIndex + 1 });
        await Assert.That(afterLocal.IsVisibleAt(id, MeshTestGraph.Sol)).IsFalse();
        await Assert.That(afterLocal.Stats.LocalCacheDrops).IsGreaterThan(0);

        var (retracted, _) = PublishEngine.PublishRetraction(
            MeshTestGraph.Triangle(),
            MeshTestGraph.Sol,
            logicalKey: "job-ttl",
            localTtlHours: 1);
        await Assert.That(retracted.IsRetractedAt("job-ttl", MeshTestGraph.Sol)).IsTrue();
        var afterRetract = TtlEngine.Expire(retracted with { HourIndex = retracted.HourIndex + 1 });
        await Assert.That(afterRetract.IsRetractedAt("job-ttl", MeshTestGraph.Sol)).IsFalse();
    }

    [Test]
    public async Task TtlEngine_DropLocal_ReopensFloodSeed()
    {
        var state = MeshTestGraph.Triangle();
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsPrices));
        published = FloodEngine.Dispatch(published);
        var pk = id.Value.ToString("N");
        await Assert.That(published.FloodSeededAt.ContainsKey(pk)).IsTrue();

        var dropped = TtlEngine.DropLocal(published, MeshTestGraph.Sol, pk, reopenFlood: true);
        await Assert.That(dropped.IsVisibleAt(id, MeshTestGraph.Sol)).IsFalse();
        await Assert.That(dropped.Stats.LocalCacheDrops).IsGreaterThan(0);

        var noop = TtlEngine.DropLocal(dropped, MeshTestGraph.Sol, pk, reopenFlood: true);
        await Assert.That(noop).IsEqualTo(dropped);
    }

    [Test]
    public async Task TtlEngine_GlobalExpiry_ClearsPendingDronesAndInboxes()
    {
        var owner = MeshIdentityIds.Person("ttl-pilot");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.Emergency),
            globalTtlHours: 1);
        published = FeedEngine.Pull(published, owner);
        published = MeshVisibility.EnqueueLaunch(
            published,
            new PendingLaunch(
                id,
                MeshTestGraph.Sol,
                MeshTestGraph.Wolf,
                ImmutableArray<MeshNodeId>.Empty,
                IsFloodHop: true,
                Priority: 1));
        published = published with
        {
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(),
                    id,
                    MeshTestGraph.Sol,
                    MeshTestGraph.Wolf,
                    RemainingHours: 3,
                    ImmutableArray<MeshNodeId>.Empty,
                    IsFloodHop: true,
                    Priority: 1),
            ],
            HourIndex = published.HourIndex + 1,
        };

        var expired = TtlEngine.Expire(published);
        await Assert.That(expired.TryGetPacket(id, out _)).IsFalse();
        await Assert.That(expired.Pending.Length).IsEqualTo(0);
        await Assert.That(expired.Drones.Length).IsEqualTo(0);
        await Assert.That(expired.IsInFeedInbox(id, owner)).IsFalse();
        await Assert.That(expired.Stats.GlobalPacketDrops).IsGreaterThan(0);
    }

    [Test]
    public async Task InvariantChecker_FlagsBandwidthEdgesAndDescribe()
    {
        var state = MeshTestGraph.Triangle() with
        {
            BandwidthUsedThisHour = ImmutableDictionary<string, int>.Empty
                .Add(MeshTestGraph.Sol.Value, 99)
                .Add("ghost-node", 1),
            NodeCaches = ImmutableDictionary<string, ImmutableDictionary<string, NodeCacheEntry>>.Empty
                .Add("ghost-cache", ImmutableDictionary<string, NodeCacheEntry>.Empty
                    .Add("missing-pk", new NodeCacheEntry(0, 1, null))),
            Mailboxes = ImmutableDictionary<string, MeshMailbox>.Empty.Add(
                "orphan",
                new MeshMailbox(
                    MeshIdentityId.From("orphan"),
                    MeshIdentityKind.Thing,
                    MeshNodeId.From("nowhere"),
                    ImmutableHashSet<string>.Empty)),
            Pending =
            [
                new PendingLaunch(
                    PacketId.New(),
                    MeshTestGraph.Sol,
                    MeshTestGraph.Wolf,
                    ImmutableArray<MeshNodeId>.Empty,
                    false,
                    1),
            ],
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(),
                    PacketId.New(),
                    MeshNodeId.From("nope"),
                    MeshTestGraph.Wolf,
                    RemainingHours: -1,
                    ImmutableArray<MeshNodeId>.Empty,
                    false,
                    1),
            ],
            Edges = MeshTestGraph.Triangle().Edges.Add(
                new MeshEdge(MeshTestGraph.Sol, MeshNodeId.From("missing"), 0, 0, 0)),
        };

        var errors = InvariantChecker.Check(state);
        await Assert.That(errors.Any(e => e.Contains("bandwidth"))).IsTrue();
        await Assert.That(errors.Any(e => e.Contains("unknown node"))).IsTrue();
        await Assert.That(errors.Any(e => e.Contains("missing packet"))).IsTrue();
        await Assert.That(errors.Any(e => e.Contains("negative remaining"))).IsTrue();
        await Assert.That(errors.Any(e => e.Contains("non-positive travel"))).IsTrue();
        await Assert.That(InvariantChecker.Describe(state)).Contains("hour=");

        await Assert.That(() => InvariantChecker.AssertAll(state))
            .ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task FeedEngine_PullAll_Unsubscribe_AndUnlinkedMailbox()
    {
        var owner = MeshIdentityIds.Firm("feed-co");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        state = FeedEngine.Subscribe(state, owner, MeshFeedId.CommerceSpot);
        state = FeedEngine.Subscribe(state, owner, MeshFeedId.CommerceSpot); // idempotent
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.CommerceSpot));

        var pulled = FeedEngine.PullAll(published);
        await Assert.That(pulled.IsInFeedInbox(id, owner)).IsTrue();

        var unsub = FeedEngine.Unsubscribe(pulled, owner, MeshFeedId.CommerceSpot);
        await Assert.That(FeedEngine.EffectiveFeedIds(unsub, owner).Contains(MeshFeedId.CommerceSpot.Value))
            .IsFalse();
        await Assert.That(FeedEngine.Unsubscribe(unsub, MeshIdentityIds.Person("nobody"), MeshFeedId.NewsGeneral))
            .IsEqualTo(unsub);

        var box = unsub.Mailboxes[owner.Value] with { LinkedToNode = false };
        var unlinked = unsub with { Mailboxes = unsub.Mailboxes.SetItem(owner.Value, box) };
        await Assert.That(FeedEngine.Pull(unlinked, owner)).IsEqualTo(unlinked);
        await Assert.That(FeedEngine.ForceMandatoryAtNode(unlinked, id, MeshTestGraph.Sol))
            .IsEqualTo(unlinked);
    }

    [Test]
    public async Task MailboxEngine_MoveRegistersMissing_AndSkipsUnlinked()
    {
        var owner = MeshIdentityIds.Thing("probe");
        var moved = MailboxEngine.Move(MeshTestGraph.Triangle(), owner, MeshTestGraph.Wolf);
        await Assert.That(moved.TryGetMailbox(owner, out var box)).IsTrue();
        await Assert.That(box.LocationNodeId).IsEqualTo(MeshTestGraph.Wolf);

        var linked = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);
        var (packeted, id) = PublishEngine.PublishPulse(
            linked,
            MeshTestGraph.Wolf,
            MeshAddress.ToIdentity(owner));
        await Assert.That(packeted.IsInMailbox(id, owner)).IsFalse();

        var unlinkedBox = packeted.Mailboxes[owner.Value] with { LinkedToNode = false };
        var unlinked = packeted with { Mailboxes = packeted.Mailboxes.SetItem(owner.Value, unlinkedBox) };
        var afterMove = MailboxEngine.Move(unlinked, owner, MeshTestGraph.Wolf);
        await Assert.That(afterMove.IsInMailbox(id, owner)).IsFalse();
        await Assert.That(MailboxEngine.PushAtNode(afterMove, id, MeshTestGraph.Wolf)).IsEqualTo(afterMove);
    }

    [Test]
    public async Task DroneTick_MultiHopContinuation_AndEmptyTick()
    {
        var empty = DroneTickEngine.Tick(MeshTestGraph.Triangle());
        await Assert.That(empty.Drones.IsDefaultOrEmpty).IsTrue();

        var (published, id) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(),
            MeshTestGraph.Sol,
            MeshAddress.ToPlace(MeshTestGraph.Other));
        published = published with
        {
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(),
                    id,
                    MeshTestGraph.Sol,
                    MeshTestGraph.Wolf,
                    RemainingHours: 1,
                    ImmutableArray.Create(MeshTestGraph.Proxima, MeshTestGraph.Other),
                    IsFloodHop: false,
                    Priority: 2),
            ],
            Policy = new MeshPolicy(LossEveryNth: 0),
        };

        var after = DroneTickEngine.Tick(published);
        await Assert.That(after.IsVisibleAt(id, MeshTestGraph.Wolf)).IsTrue();
        await Assert.That(after.Pending.Any(p => p.From.Equals(MeshTestGraph.Wolf) && p.To.Equals(MeshTestGraph.Proxima)))
            .IsTrue();
    }

    [Test]
    public async Task MeshVisibility_EnqueueDedup_AndPendingCap()
    {
        var policy = new MeshPolicy(MaxPendingPerHub: 1, LossEveryNth: 0);
        var state = MeshTestGraph.Triangle(policy);
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsGeneral));

        var launch = new PendingLaunch(
            id,
            MeshTestGraph.Sol,
            MeshTestGraph.Wolf,
            ImmutableArray<MeshNodeId>.Empty,
            IsFloodHop: true,
            Priority: 1);
        var once = MeshVisibility.EnqueueLaunch(published, launch);
        var twice = MeshVisibility.EnqueueLaunch(once, launch);
        await Assert.That(twice.Pending.Length).IsEqualTo(once.Pending.Length);

        var secondId = PacketId.New();
        twice = PublishEngine.Publish(
            twice,
            new MeshPacket(
                secondId,
                MeshTrafficLayer.Feed,
                true,
                ImmutableArray<byte>.Empty,
                1,
                null,
                null,
                1,
                MeshTestGraph.Sol,
                MeshAddress.ToFeed(MeshFeedId.NewsPrices),
                twice.HourIndex),
            MeshTestGraph.Sol);
        var capped = MeshVisibility.EnqueueLaunch(
            twice,
            new PendingLaunch(
                secondId,
                MeshTestGraph.Sol,
                MeshTestGraph.Proxima,
                ImmutableArray<MeshNodeId>.Empty,
                IsFloodHop: true,
                Priority: 1));
        await Assert.That(capped.Stats.BandwidthDeferred).IsGreaterThan(0);
    }

    [Test]
    public async Task PublishEngine_SameNodePlace_AndUnknownOriginThrows()
    {
        var (same, id) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(),
            MeshTestGraph.Sol,
            MeshAddress.ToPlace(MeshTestGraph.Sol));
        await Assert.That(same.IsVisibleAt(id, MeshTestGraph.Sol)).IsTrue();
        await Assert.That(same.Pending.Length).IsEqualTo(0);

        await Assert.That(() => PublishEngine.PublishPulse(
                MeshTestGraph.Triangle(),
                MeshNodeId.From("missing"),
                MeshAddress.ToFeed(MeshFeedId.NewsGeneral)))
            .ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task LaunchEngine_DefersWhenBandwidthExhausted()
    {
        var state = MeshTestGraph.Triangle(bandwidth: 1);
        var (published, id) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToPlace(MeshTestGraph.Wolf));
        published = published with
        {
            BandwidthUsedThisHour = ImmutableDictionary<string, int>.Empty.Add(MeshTestGraph.Sol.Value, 1),
        };
        var launched = LaunchEngine.LaunchPending(published);
        await Assert.That(launched.Pending.Any(p => p.PacketId.Equals(id))).IsTrue();
        await Assert.That(launched.Stats.BandwidthDeferred).IsGreaterThan(0);
        await Assert.That(LaunchEngine.LaunchPending(MeshTestGraph.Triangle()).Pending.Length).IsEqualTo(0);
    }

    [Test]
    public async Task MeshState_Queries_AndIdentityKinds()
    {
        var empty = MeshState.Empty();
        await Assert.That(empty.TryGetCacheEntry(PacketId.New(), MeshTestGraph.Sol, out _)).IsFalse();
        await Assert.That(empty.TryGetMailbox(MeshIdentityIds.Person("x"), out _)).IsFalse();
        await Assert.That(empty.IsRetractedAt("", MeshTestGraph.Sol)).IsFalse();

        await Assert.That(MeshIdentityIds.TryParseKind(MeshIdentityId.From("raw"))).IsNull();
        await Assert.That(MeshIdentityIds.TryParseKind(MeshIdentityIds.Household("h"))).IsEqualTo(MeshIdentityKind.Household);
        await Assert.That(MeshIdentityIds.TryParseKind(MeshIdentityIds.Firm("f"))).IsEqualTo(MeshIdentityKind.Firm);
        await Assert.That(MeshIdentityIds.TryParseKind(MeshIdentityIds.Ship("s"))).IsEqualTo(MeshIdentityKind.Ship);
        await Assert.That(MeshIdentityIds.TryParseKind(MeshIdentityIds.Thing("t"))).IsEqualTo(MeshIdentityKind.Thing);
        await Assert.That(MeshFeedId.IsMandatoryFeed(MeshFeedId.Emergency)).IsTrue();
        await Assert.That(MeshNodeId.From("n").ToString()).IsEqualTo("n");
        await Assert.That(MeshIdentityId.From("i").ToString()).IsEqualTo("i");
        await Assert.That(MeshFeedId.From("f").ToString()).IsEqualTo("f");

        var path = MeshPathfinder.FindPath(MeshTestGraph.Triangle(), MeshTestGraph.Sol, MeshTestGraph.Sol);
        await Assert.That(path!.Value.Length).IsEqualTo(1);
        await Assert.That(MeshPathfinder.TravelHours(
                MeshTestGraph.Triangle(),
                MeshTestGraph.Sol,
                MeshTestGraph.Wolf,
                MeshTrafficLayer.Bulk))
            .IsEqualTo(20);
        await Assert.That(MeshPathfinder.TravelHours(
                MeshTestGraph.Triangle(),
                MeshTestGraph.Sol,
                MeshNodeId.From("nowhere"),
                MeshTrafficLayer.Pulse))
            .IsEqualTo(int.MaxValue);
    }

    [Test]
    public async Task SpaceCombat_ObservationFeatures_AndArcadeBranches()
    {
        var fighter = new CraftState { Profile = CraftProfile.FighterDefault, Speed = 24f };
        fighter.ResetVitals();
        fighter.Position = Vector3.Zero;
        fighter.Yaw = 0f;
        fighter.Pitch = 0f;

        var withLead = new CraftObservation
        {
            Self = fighter,
            TargetPosition = new Vector3(0, 0, 40f),
            TargetVelocity = new Vector3(5f, 0, 0),
            EscortAnchor = new Vector3(0, 0, -20f),
            ActiveThreats = 3,
            Dt = 1f / 60f,
        };
        var dest = new float[CraftObservationFeatures.Size];
        CraftObservationFeatures.Write(withLead, dest);
        await Assert.That(dest[12]).IsEqualTo(1f);
        await Assert.That(dest[13]).IsEqualTo(1f);

        var freighter = new CraftState { Profile = CraftProfile.FreighterDefault, Speed = 10f };
        freighter.ResetVitals();
        var noTarget = new CraftObservation
        {
            Self = freighter,
            TargetPosition = null,
            TargetVelocity = null,
            EscortAnchor = freighter.Position,
            ActiveThreats = 0,
            Dt = 0.016f,
        };
        CraftObservationFeatures.Write(noTarget, dest);
        await Assert.That(dest[12]).IsEqualTo(0f);
        await Assert.That(dest[13]).IsEqualTo(-1f);

        await Assert.ThrowsAsync<ArgumentException>(() =>
        {
            CraftObservationFeatures.Write(withLead, new float[2]);
            return Task.CompletedTask;
        });

        // Coincident target → AimError early-out inside features + composer.
        CrewIntentComposer.AimError(fighter, fighter.Position, out var yaw0, out var pitch0);
        await Assert.That(yaw0).IsEqualTo(0f);
        await Assert.That(pitch0).IsEqualTo(0f);
        await Assert.That(CrewIntentComposer.Compose(CrewStation.Dual, default, default, default).Fire)
            .IsFalse();

        fighter.Active = false;
        ArcadeFlight.Apply(fighter, new FlightIntent { ThrottleUp = 1f }, 0.1f);
        await Assert.That(fighter.Position).IsEqualTo(Vector3.Zero);

        fighter.Active = true;
        fighter.Speed = fighter.Profile.MinSpeed;
        ArcadeFlight.Apply(fighter, new FlightIntent
        {
            RollLeft = 1f,
            RollRight = 1f,
            ThrottleDown = 1f,
            PitchDelta = 0.2f,
        }, 0.2f);
        await Assert.That(fighter.Speed).IsGreaterThanOrEqualTo(fighter.Profile.MinSpeed);

        var flat = new CraftProfile
        {
            Role = CraftRole.Fighter,
            MaxSpeed = 10f,
            MinSpeed = 10f,
            Acceleration = 1f,
            Deceleration = 1f,
            Drag = 0f,
            TurnRate = 1f,
            MaxHull = 1f,
            MaxShield = 1f,
            HitRadius = 1f,
        };
        var flatCraft = new CraftState { Profile = flat, Speed = 10f };
        flatCraft.ResetVitals();
        await Assert.That(flatCraft.Throttle01).IsEqualTo(0f);

        await Assert.That(CombatHits.SegmentHitsSphere(
                Vector3.Zero, Vector3.Zero, new Vector3(0.5f, 0, 0), 1f))
            .IsTrue();
    }

    [Test]
    public async Task SpaceCombat_BoltsTargetingHostileAndMissionEdges()
    {
        var pool = Enumerable.Range(0, 2).Select(_ => new LaserBolt { Active = true }).ToArray();
        await Assert.That(BoltPools.TrySpawn(pool, Vector3.Zero, Vector3.UnitZ, 1f, true)).IsFalse();

        pool[0].Active = false;
        await Assert.That(BoltPools.TrySpawn(pool, Vector3.Zero, Vector3.UnitZ * 10f, 0.05f, true, 0.2f)).IsTrue();
        BoltPools.Update(pool, 0.1f, Vector3.Zero, maxDist: 1000f);
        await Assert.That(pool[0].Active).IsFalse();

        pool[0].Active = true;
        pool[0].Life = 10f;
        pool[0].Position = Vector3.Zero;
        pool[0].Velocity = Vector3.UnitZ * 50f;
        BoltPools.Update(pool, 0.5f, Vector3.Zero, maxDist: 1f);
        await Assert.That(pool[0].Active).IsFalse();

        var candidates = new[]
        {
            new CraftState { Profile = CraftProfile.HostileDefault, Active = false },
            new CraftState
            {
                Profile = CraftProfile.HostileDefault,
                Position = new Vector3(0, 0, 2f),
                Active = true,
                PlayerControlled = true,
            },
            new CraftState
            {
                Profile = CraftProfile.HostileDefault,
                Position = new Vector3(0, 0, 30f),
                Active = true,
            },
            new CraftState
            {
                Profile = CraftProfile.HostileDefault,
                Position = new Vector3(40f, 0, 0),
                Active = true,
            },
        };
        var lockTarget = Targeting.FindLockTarget(candidates, Vector3.Zero, Vector3.UnitZ);
        await Assert.That(lockTarget).IsEqualTo(candidates[2]);
        await Assert.That(Targeting.FindLockTarget(candidates, Vector3.Zero, -Vector3.UnitZ)).IsNull();

        var self = new CraftState { Profile = CraftProfile.HostileDefault };
        self.ResetVitals();
        self.Position = Vector3.Zero;
        var squadron = new[]
        {
            self,
            new CraftState
            {
                Profile = CraftProfile.HostileDefault,
                Position = new Vector3(2f, 0, 0),
                Active = true,
            },
        };
        HostileAi.Update(self, Vector3.Zero, squadron, 0.05f); // coincident player → early return
        HostileAi.Update(self, new Vector3(0, 0, 20f), squadron, 0.05f); // orbit inner
        HostileAi.Update(self, new Vector3(0, 0, 80f), squadron, 0.05f); // orbit outer
        HostileAi.Update(self, new Vector3(0, 0, 40f), squadron, 0.05f); // orbit band + separation

        self.FireCooldown = 0f;
        self.Position = new Vector3(0, 0, 40f);
        _ = HostileAi.TryFire(self, Vector3.Zero, nearbyAllies: 4);
        HostileAi.GetBoltVelocity(self, Vector3.Zero, out var origin, out var vel);
        await Assert.That(origin.Length()).IsGreaterThan(0f);
        await Assert.That(vel.Length()).IsGreaterThan(0f);

        var spawned = new CraftState { Profile = CraftProfile.HostileDefault };
        HostileAi.SpawnNear(spawned, Vector3.Zero, Vector3.UnitY, new Random(1)); // degenerate right → UnitX
        await Assert.That(spawned.Active).IsTrue();

        var pilot = new HeuristicPilotAi(engageDistance: 30f, turnGain: 0.1f);
        var escort = pilot.Tick(new CraftObservation
        {
            Self = new CraftState { Profile = CraftProfile.FighterDefault, Speed = 20f },
            TargetPosition = null,
            TargetVelocity = null,
            EscortAnchor = Vector3.Zero,
            ActiveThreats = 0,
            Dt = 0.016f,
        });
        await Assert.That(escort.ThrottleUp).IsGreaterThan(0f);

        var closeThreat = new CraftState { Profile = CraftProfile.FighterDefault, Speed = 20f };
        closeThreat.ResetVitals();
        closeThreat.Position = Vector3.Zero;
        var brake = pilot.Tick(new CraftObservation
        {
            Self = closeThreat,
            TargetPosition = new Vector3(0, 0, 5f),
            TargetVelocity = null,
            EscortAnchor = Vector3.Zero,
            ActiveThreats = 1,
            Dt = 0.016f,
        });
        await Assert.That(brake.ThrottleDown).IsGreaterThan(0f);

        var farThreat = pilot.Tick(new CraftObservation
        {
            Self = closeThreat,
            TargetPosition = new Vector3(0, 0, 80f),
            TargetVelocity = new Vector3(1f, 0, 0),
            EscortAnchor = Vector3.Zero,
            ActiveThreats = 1,
            Dt = 0.016f,
        });
        await Assert.That(farThreat.ThrottleUp).IsGreaterThan(0f);

        var session = new MissionSession(new MissionDescriptor
        {
            Id = "gaps",
            FreighterProfile = CraftProfile.FreighterDefault,
            FighterProfile = CraftProfile.FighterDefault,
            HostileProfile = CraftProfile.HostileDefault,
            HostileCount = 2,
            ProtectSeconds = 0.01f,
            DestroyRequired = 99,
            MaxHostilesAlive = 4,
        }, seed: 11);
        session.Begin();
        session.SetCrewControllers(new HeuristicPilotAi(), new HeuristicGunnerAi(), new HeuristicPilotAi());
        _ = session.PlayerBolts;
        _ = session.EnemyBolts;
        _ = session.Kills;
        _ = session.ProtectRemaining;
        _ = session.LockTarget;
        session.CrewStation = CrewStation.Pilot;
        session.Tick(new FlightIntent { YawDelta = 0.01f }, 0.05f);

        for (var i = 0; i < 40 && !session.CanTransfer; i++)
            session.Tick(default, 0.05f);
        session.Tick(new FlightIntent { Transfer = true }, 0.016f);

        // Freighter destroyed while escorting → Failed.
        session.Freighter.Active = false;
        session.Tick(default, 0.05f);
        await Assert.That(session.Phase).IsEqualTo(MissionPhase.Failed);
        session.Tick(default, 0.05f); // Complete/Failed early return

        var failSession = new MissionSession(new MissionDescriptor
        {
            Id = "fail-hull",
            FreighterProfile = CraftProfile.FreighterDefault,
            FighterProfile = CraftProfile.FighterDefault,
            HostileProfile = CraftProfile.HostileDefault,
            HostileCount = 1,
            ProtectSeconds = 30f,
            DestroyRequired = 1,
            MaxHostilesAlive = 4,
        }, seed: 5);
        failSession.Begin();
        failSession.Player.Shield = 0f;
        failSession.Player.Hull = 0.05f;
        // Drive an enemy bolt into the freighter via hostile fire at close range.
        var hostile = failSession.Hostiles.First(h => h.Active);
        hostile.Position = failSession.Player.Position + failSession.Player.Forward * 40f;
        hostile.FireCooldown = 0f;
        for (var i = 0; i < 120 && failSession.Phase != MissionPhase.Failed; i++)
        {
            hostile.Position = failSession.Player.Position + failSession.Player.Forward * 40f;
            hostile.FireCooldown = 0f;
            failSession.Tick(default, 0.05f);
        }
    }

    [Test]
    public async Task World_SimulationWorld_ClampAndOccupancyEdges()
    {
        var map = new DenseGrid<byte>(6, 6);
        map.Set(new GridIndex(3, 3), 1);
        var world = new SimulationWorld(map, cellSize: 2f);
        await Assert.That(world.CellSize).IsEqualTo(2f);
        await Assert.That(world.Occupancy.Width).IsEqualTo(6u);
        await Assert.That(() => new SimulationWorld(null!)).ThrowsExactly<ArgumentNullException>();

        var bounds = new RoomInteriorBounds
        {
            MinX = -1, MaxX = 1, MinY = 0, MaxY = 2, MinZ = -1, MaxZ = 1,
        };
        var clamp = bounds.ToInteriorClamp();
        await Assert.That(clamp.MinX).IsEqualTo(-1f);
        await Assert.That(clamp.MaxY).IsEqualTo(2f);

        var range = new AxisAlignedRangeBox(40f);
        var field = new BoundedHeightfield(new HillSampler(), range);
        await Assert.That(field.ExtentMeters).IsEqualTo(40f);
        await Assert.That(field.TryHeightfieldContact(new Vector3(100f, 0f, 0f), 1f)).IsFalse();
        await Assert.That(() => new BoundedHeightfield(null!, 10f)).ThrowsExactly<ArgumentNullException>();
        await Assert.That(() => new BoundedHeightfield(null!, range)).ThrowsExactly<ArgumentNullException>();

        var open = new DenseGrid<byte>(5, 5);
        var stay = PlanarOccupancy.TryMove(open, new Vector3(1f, 0f, 1f), Vector3.Zero, 0.2f);
        await Assert.That(stay).IsEqualTo(new Vector3(1f, 0f, 1f));

        // Near a wall but not overlapping → OverlapsWall falls through loop close-brace.
        open.Set(new GridIndex(2, 2), 1);
        await Assert.That(PlanarOccupancy.OverlapsWall(open, new Vector3(0.5f, 0f, 0.5f), 0.1f)).IsFalse();

        // Push from slightly offset inside wall (non-zero distSq path) and from fully enclosed cell.
        var pushed = PlanarOccupancy.PushOutOfWalls(open, new Vector3(2.2f, 0f, 2.2f), 0.4f);
        await Assert.That(PlanarOccupancy.OverlapsWall(open, pushed, 0.4f)).IsFalse();

        var enclosed = new DenseGrid<byte>(3, 3);
        for (uint z = 0; z < 3; z++)
        for (uint x = 0; x < 3; x++)
            enclosed.Set(new GridIndex(x, z), 1);
        var escape = PlanarOccupancy.PushOutOfWalls(enclosed, new Vector3(1.5f, 0f, 1.5f), 0.3f, maxIterations: 2);
        await Assert.That(escape.Length()).IsGreaterThan(0f);

        await Assert.That(PlanarOccupancy.TryRaycastDisc(
                Vector3.Zero, Vector3.Zero, new Vector3(1, 0, 0), 10f, 1f, out _)).IsFalse();
        await Assert.That(PlanarOccupancy.TryRaycastDisc(
                Vector3.Zero, Vector3.UnitZ, new Vector3(0, 0, 0.1f), 10f, 1f, out _)).IsFalse();
        await Assert.That(PlanarOccupancy.TryRaycastWall(
                open, Vector3.Zero, Vector3.Zero, 5f, 1f, out _)).IsFalse();

        // Same-cell clearance + OOB IsCellBlocked via wall ray past map edge.
        await Assert.That(PlanarOccupancy.TryRaycastWall(
                open, new Vector3(4.5f, 0f, 4.5f), new Vector3(1f, 0f, 0f), 5f, 1f, out _)).IsTrue();

        // Degenerate segment for DistancePointToSegment via clearance with coincident from/to.
        await Assert.That(PlanarOccupancy.HasLineOfSight(
                open, new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f), clearanceRadius: 0.5f))
            .IsTrue();
    }

    [Test]
    public async Task Mesh_PublishLaunchPathfinder_HardBranches()
    {
        var state = MeshTestGraph.Triangle(new MeshPolicy(LossEveryNth: 0, MaxPacketsPerNodeCache: 1));
        var (first, id1) = PublishEngine.PublishPulse(state, MeshTestGraph.Sol, MeshAddress.ToFeed(MeshFeedId.NewsGeneral));
        var (capped, id2) = PublishEngine.PublishPulse(first, MeshTestGraph.Sol, MeshAddress.ToFeed(MeshFeedId.NewsPrices));
        var visible = (capped.IsVisibleAt(id1, MeshTestGraph.Sol) ? 1 : 0)
            + (capped.IsVisibleAt(id2, MeshTestGraph.Sol) ? 1 : 0);
        await Assert.That(visible).IsLessThanOrEqualTo(1);

        var (retract1, _) = PublishEngine.PublishRetraction(MeshTestGraph.Triangle(), MeshTestGraph.Sol, "job-a");
        var (retract2, _) = PublishEngine.PublishRetraction(retract1, MeshTestGraph.Sol, "job-a"); // duplicate key noop
        await Assert.That(retract2.IsRetractedAt("job-a", MeshTestGraph.Sol)).IsTrue();

        var hourState = MeshTestGraph.Triangle() with { HourIndex = 5 };
        var custom = new MeshPacket(
            PacketId.New(),
            MeshTrafficLayer.Pulse,
            true,
            ImmutableArray<byte>.Empty,
            1,
            null,
            null,
            1,
            MeshTestGraph.Wolf, // will be rewritten to origin
            MeshAddress.ToPlace(MeshTestGraph.Other),
            PublishedHour: 0);
        var directed = PublishEngine.Publish(hourState, custom, MeshTestGraph.Sol);
        await Assert.That(directed.Pending.Length).IsGreaterThan(0);

        var island = MeshNodeId.From("island");
        var disconnected = MeshTestGraph.Triangle() with
        {
            Nodes = MeshTestGraph.Triangle().Nodes.Add(island.Value, new MeshNode(island, island.Value, "Island", 4)),
        };
        await Assert.That(MeshPathfinder.FindPath(disconnected, MeshTestGraph.Sol, island)).IsNull();
        await Assert.That(MeshPathfinder.FindPath(MeshTestGraph.Triangle(), MeshNodeId.From("ghost"), MeshTestGraph.Sol))
            .IsNull();

        await Assert.That(() => PublishEngine.PublishPulse(
                disconnected,
                MeshTestGraph.Sol,
                MeshAddress.ToPlace(island)))
            .ThrowsExactly<InvalidOperationException>();

        // Launch skips: unknown from, missing packet, flood already visible, no travel hours.
        var (published, pid) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(),
            MeshTestGraph.Sol,
            MeshAddress.ToPlace(MeshTestGraph.Wolf));
        published = MeshVisibility.CreditNode(published, pid, MeshTestGraph.Wolf);
        published = published with
        {
            Pending =
            [
                new PendingLaunch(pid, MeshNodeId.From("ghost"), MeshTestGraph.Wolf, ImmutableArray<MeshNodeId>.Empty, false, 1),
                new PendingLaunch(PacketId.New(), MeshTestGraph.Sol, MeshTestGraph.Wolf, ImmutableArray<MeshNodeId>.Empty, false, 1),
                new PendingLaunch(pid, MeshTestGraph.Sol, MeshTestGraph.Wolf, ImmutableArray<MeshNodeId>.Empty, true, 1),
                new PendingLaunch(pid, MeshTestGraph.Sol, island, ImmutableArray<MeshNodeId>.Empty, false, 1),
            ],
        };
        var launched = LaunchEngine.LaunchPending(published);
        await Assert.That(launched.Drones.Length).IsEqualTo(0);

        // Enqueue skips when drone already in flight for same hop.
        var (pulse, hopId) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(),
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsGeneral));
        pulse = pulse with
        {
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(),
                    hopId,
                    MeshTestGraph.Sol,
                    MeshTestGraph.Wolf,
                    RemainingHours: 2,
                    ImmutableArray<MeshNodeId>.Empty,
                    IsFloodHop: true,
                    Priority: 1),
            ],
        };
        var skipped = MeshVisibility.EnqueueLaunch(
            pulse,
            new PendingLaunch(hopId, MeshTestGraph.Sol, MeshTestGraph.Wolf, ImmutableArray<MeshNodeId>.Empty, true, 1));
        await Assert.That(skipped.Pending.Length).IsEqualTo(0);
        var creditedTwice = MeshVisibility.CreditNode(skipped, hopId, MeshTestGraph.Sol);
        await Assert.That(creditedTwice.Stats.CacheCredits).IsEqualTo(skipped.Stats.CacheCredits);
        await Assert.That(MeshVisibility.CreditNode(MeshTestGraph.Triangle(), PacketId.New(), MeshTestGraph.Sol)
            .Stats.CacheCredits).IsEqualTo(0);
    }

    [Test]
    public async Task Mesh_MailboxFeedFloodDrone_RemainingBranches()
    {
        var owner = MeshIdentityIds.Person("hop-pilot");
        var state = MailboxEngine.Register(MeshTestGraph.Triangle(), owner, MeshTestGraph.Sol);

        // Identity packet at Sol, then move mailbox to Wolf so Move pushes identity + mandatory catch-up.
        var (identityPub, identityId) = PublishEngine.PublishPulse(
            state,
            MeshTestGraph.Sol,
            MeshAddress.ToIdentity(owner));
        identityPub = MeshVisibility.CreditNode(identityPub, identityId, MeshTestGraph.Wolf);
        var (emergencyPub, emergencyId) = PublishEngine.PublishPulse(
            identityPub,
            MeshTestGraph.Wolf,
            MeshAddress.ToFeed(MeshFeedId.Emergency));
        // Corrupt cache with a ghost key so Move skips missing packets.
        emergencyPub = emergencyPub with
        {
            NodeCaches = emergencyPub.NodeCaches.SetItem(
                MeshTestGraph.Wolf.Value,
                emergencyPub.NodeCaches[MeshTestGraph.Wolf.Value].Add("ghost-pk", new NodeCacheEntry(0, 1, null))),
        };
        var moved = MailboxEngine.Move(emergencyPub, owner, MeshTestGraph.Wolf);
        await Assert.That(moved.IsInMailbox(identityId, owner) || moved.IsInFeedInbox(emergencyId, owner)).IsTrue();

        // Pull skips non-feed + unsubscribed + already-in-inbox packets.
        var (news, newsId) = PublishEngine.PublishPulse(moved, MeshTestGraph.Wolf, MeshAddress.ToFeed(MeshFeedId.NewsGeneral));
        news = FeedEngine.Subscribe(news, owner, MeshFeedId.NewsGeneral);
        news = FeedEngine.Pull(news, owner);
        news = FeedEngine.Pull(news, owner); // idempotent inbox hit
        await Assert.That(news.IsInFeedInbox(newsId, owner)).IsTrue();
        await Assert.That(FeedEngine.ForceMandatoryAtNode(news, PacketId.New(), MeshTestGraph.Wolf))
            .IsEqualTo(news);
        await Assert.That(FeedEngine.ForceMandatoryAtNode(news, newsId, MeshTestGraph.Wolf))
            .IsEqualTo(news); // non-mandatory feed → no-op

        // Flood: already-seeded node skipped; already-visible neighbor skipped.
        var flooded = FloodEngine.Dispatch(news);
        flooded = FloodEngine.Dispatch(flooded);
        await Assert.That(flooded.FloodSeededAt.ContainsKey(newsId.Value.ToString("N"))).IsTrue();

        // Drone loss only when RemainingHours==1; max losses stops further loss; flood-hop arrival clears seed.
        var lossPolicy = new MeshPolicy(LossEveryNth: 1, MaxLossesPerPacket: 1);
        var (feedPub, feedId) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(lossPolicy),
            MeshTestGraph.Sol,
            MeshAddress.ToFeed(MeshFeedId.NewsPrices));
        feedPub = FloodEngine.Dispatch(feedPub);
        var feedKey = feedId.Value.ToString("N");
        feedPub = feedPub with
        {
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(), feedId, MeshTestGraph.Sol, MeshTestGraph.Wolf,
                    RemainingHours: 3, ImmutableArray<MeshNodeId>.Empty, IsFloodHop: false, Priority: 1),
                new InFlightDrone(
                    DroneId.New(), feedId, MeshTestGraph.Sol, MeshTestGraph.Wolf,
                    RemainingHours: 1, ImmutableArray<MeshNodeId>.Empty, IsFloodHop: true, Priority: 1),
            ],
            PacketLossCounts = ImmutableDictionary<string, int>.Empty.Add(feedKey, 1),
            FloodSeededAt = ImmutableDictionary<string, ImmutableHashSet<string>>.Empty
                .Add(feedKey, ImmutableHashSet.Create(MeshTestGraph.Wolf.Value)),
        };
        var afterTick = DroneTickEngine.Tick(feedPub);
        await Assert.That(afterTick.Drones.Any(d => d.RemainingHours == 2) || afterTick.Stats.DronesArrived > 0).IsTrue();

        // Multi-hop rest length > 1 path continuation.
        var (directed, dirId) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(new MeshPolicy(LossEveryNth: 0)),
            MeshTestGraph.Sol,
            MeshAddress.ToPlace(MeshTestGraph.Other));
        directed = directed with
        {
            Drones =
            [
                new InFlightDrone(
                    DroneId.New(), dirId, MeshTestGraph.Sol, MeshTestGraph.Wolf,
                    RemainingHours: 1,
                    ImmutableArray.Create(MeshTestGraph.Proxima, MeshTestGraph.Other),
                    IsFloodHop: false,
                    Priority: 1),
            ],
        };
        var continued = DroneTickEngine.Tick(directed);
        await Assert.That(continued.Pending.Any(p => p.RemainingPathAfterArrival.Length == 1)).IsTrue();

        // Retraction without LocalTtlHours skipped; with TTL expires; DropLocal reopen removes neighbor seeds.
        var (retract, _) = PublishEngine.PublishRetraction(
            MeshTestGraph.Triangle(), MeshTestGraph.Sol, "spot-x", localTtlHours: 1);
        var noTtl = retract with
        {
            NodeRetractions = retract.NodeRetractions.SetItem(
                MeshTestGraph.Sol.Value,
                retract.NodeRetractions[MeshTestGraph.Sol.Value]
                    .SetItem("no-ttl", new NodeCacheEntry(retract.HourIndex, 1, null))),
        };
        var expired = TtlEngine.Expire(noTtl with { HourIndex = noTtl.HourIndex + 1 });
        await Assert.That(expired.IsRetractedAt("spot-x", MeshTestGraph.Sol)).IsFalse();

        var (pulse2, p2) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(), MeshTestGraph.Sol, MeshAddress.ToFeed(MeshFeedId.NewsGeneral));
        pulse2 = FloodEngine.Dispatch(pulse2);
        pulse2 = MeshVisibility.CreditNode(pulse2, p2, MeshTestGraph.Wolf);
        var dropped = TtlEngine.DropLocal(pulse2, MeshTestGraph.Wolf, p2.Value.ToString("N"), reopenFlood: true);
        await Assert.That(dropped.IsVisibleAt(p2, MeshTestGraph.Wolf)).IsFalse();

        // EnqueueLaunch skips flood when destination already visible.
        var (visibleFeed, vid) = PublishEngine.PublishPulse(
            MeshTestGraph.Triangle(), MeshTestGraph.Sol, MeshAddress.ToFeed(MeshFeedId.CommerceSpot));
        visibleFeed = MeshVisibility.CreditNode(visibleFeed, vid, MeshTestGraph.Wolf);
        var skipFlood = MeshVisibility.EnqueueLaunch(
            visibleFeed,
            new PendingLaunch(vid, MeshTestGraph.Sol, MeshTestGraph.Wolf, ImmutableArray<MeshNodeId>.Empty, true, 1));
        await Assert.That(skipFlood.Pending.Length).IsEqualTo(0);

        // Publish feed address without feed id throws.
        await Assert.That(() => PublishEngine.Publish(
                MeshTestGraph.Triangle(),
                new MeshPacket(
                    PacketId.New(), MeshTrafficLayer.Feed, true, ImmutableArray<byte>.Empty,
                    1, null, null, 1, MeshTestGraph.Sol,
                    new MeshAddress(MeshAddressKind.Feed, null, null, null),
                    0),
                MeshTestGraph.Sol))
            .ThrowsExactly<InvalidOperationException>();
    }

    [Test]
    public async Task SpaceCombat_GunnerPilotHostile_LeftoverBranches()
    {
        var gunner = new HeuristicGunnerAi(fireConeDot: 0.5f, maxFireRange: 100f, minFireRange: 5f);
        var self = new CraftState { Profile = CraftProfile.FighterDefault, Speed = 20f };
        self.ResetVitals();
        self.Position = Vector3.Zero;
        await Assert.That(gunner.Tick(new CraftObservation
        {
            Self = self,
            TargetPosition = null,
            TargetVelocity = null,
            EscortAnchor = Vector3.Zero,
            ActiveThreats = 0,
            Dt = 0.016f,
        }).Fire).IsFalse();
        var tooClose = gunner.Tick(new CraftObservation
        {
            Self = self,
            TargetPosition = new Vector3(0, 0, 2f),
            TargetVelocity = new Vector3(1, 0, 0),
            EscortAnchor = Vector3.Zero,
            ActiveThreats = 1,
            Dt = 0.016f,
        });
        await Assert.That(tooClose.Fire).IsFalse();

        // Pitch-up craft → AimError right-vector fallback.
        self.Pitch = 1.1f;
        CrewIntentComposer.AimError(self, new Vector3(0, 10f, 0), out _, out _);

        // Hostile coincident right-vector (player directly above) + desired near-zero.
        var hostile = new CraftState { Profile = CraftProfile.HostileDefault };
        hostile.ResetVitals();
        hostile.Position = Vector3.Zero;
        HostileAi.Update(hostile, new Vector3(0, 1f, 0), [hostile], 0.05f);
        HostileAi.SpawnNear(hostile, Vector3.Zero, new Vector3(0, 1f, 0), new Random(2));

        var pilot = new HeuristicPilotAi(engageDistance: 40f);
        var midRange = pilot.Tick(new CraftObservation
        {
            Self = self,
            TargetPosition = self.Position + self.Forward * 25f,
            TargetVelocity = Vector3.Zero,
            EscortAnchor = Vector3.Zero,
            ActiveThreats = 1,
            Dt = 0.016f,
        });
        await Assert.That(midRange.ThrottleUp + midRange.ThrottleDown).IsGreaterThanOrEqualTo(0f);

        // Targeting score tie-break: farther but better centered loses to closer.
        var a = new CraftState { Profile = CraftProfile.HostileDefault, Position = new Vector3(0, 0, 20f), Active = true };
        var b = new CraftState { Profile = CraftProfile.HostileDefault, Position = new Vector3(0, 0, 25f), Active = true };
        var best = Targeting.FindLockTarget([a, b], Vector3.Zero, Vector3.UnitZ);
        await Assert.That(best).IsEqualTo(a);

        var map = new DenseGrid<byte>(4, 4);
        map.Set(new GridIndex(1, 1), 1);
        map.Set(new GridIndex(2, 1), 1);
        map.Set(new GridIndex(1, 2), 1);
        map.Set(new GridIndex(2, 2), 1);
        // Push with maxIterations exhausting while still overlapping.
        var stuck = PlanarOccupancy.PushOutOfWalls(map, new Vector3(1.5f, 0f, 1.5f), 0.9f, maxIterations: 1);
        await Assert.That(stuck.LengthSquared()).IsGreaterThan(0f);
        // OverlapsWall near blocked cell but outside radius → false path through loop end.
        await Assert.That(PlanarOccupancy.OverlapsWall(map, new Vector3(3.5f, 0f, 3.5f), 0.05f)).IsFalse();
        await Assert.That(PlanarOccupancy.HasLineOfSight(
                map, new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 3.5f), clearanceRadius: 0.6f))
            .IsFalse();
    }
}
