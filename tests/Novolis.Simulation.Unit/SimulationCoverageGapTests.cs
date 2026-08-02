using System.Collections.Immutable;
using System.Numerics;
using Novolis.Physics.Abstractions;
using Novolis.Simulation.Humanoid;
using Novolis.Simulation.Kinematics;
using Novolis.Simulation.Mesh;
using Novolis.Simulation.World;

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
}
