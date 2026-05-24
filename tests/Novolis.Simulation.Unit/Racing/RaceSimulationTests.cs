namespace Novolis.Simulation.Racing.Tests;

using System.Numerics;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Race;
using Novolis.Simulation.Racing.Rewards;
using Novolis.Simulation.Racing.Sensors;
using Novolis.Simulation.Racing.Tracks;

using TUnit.Assertions;

public sealed class RaceSimulationTests
{
    private sealed class FullThrottleController : IRaceCarController
    {
        public string Name => "FullThrottle";
        public CarVisualStyle VisualStyle => new("🚗", "#FF0000");
        public CarControlDecision Decide(in CarObservation obs) => new(0, 1.0, 0);
    }

    private sealed class IdleController : IRaceCarController
    {
        public string Name => "Idle";
        public CarVisualStyle VisualStyle => new("🚙", "#0000FF");
        public CarControlDecision Decide(in CarObservation obs) => new(0, 0, 0);
    }

    private sealed class LeftTurnController : IRaceCarController
    {
        public string Name => "Left";
        public CarVisualStyle VisualStyle => new("🏎", "#00FF00");
        public CarControlDecision Decide(in CarObservation obs) => new(-1.0, 0.5, 0);
    }

    private sealed class SlowLeftController : IRaceCarController
    {
        public string Name => "SlowLeft";
        public CarVisualStyle VisualStyle => new("🚕", "#FFFF00");
        public CarControlDecision Decide(in CarObservation obs) => new(-0.3, 0.2, 0);
    }

    private static readonly RaceTrack _track = new TrackBuilder().Build(new Novolis.Simulation.Racing.Tracks.CircleTrack());

    private static RaceSimulation BuildSim(params IRaceCarController[] controllers) =>
        new(_track, controllers);

    [Test]
    public async Task InitialState_Tick_IsZero()
    {
        var sim = BuildSim(new IdleController());
        await Assert.That(sim.State.Tick).IsEqualTo(0);
    }

    [Test]
    public async Task InitialState_Cars_CountMatchesControllers()
    {
        var sim = BuildSim(new IdleController(), new FullThrottleController(), new LeftTurnController());
        await Assert.That(sim.State.Cars.Count).IsEqualTo(3);
    }

    [Test]
    public async Task InitialState_AllCars_SpeedIsZero()
    {
        var sim = BuildSim(new FullThrottleController(), new IdleController());
        await Assert.That(sim.State.Cars.All(c => c.Speed == 0)).IsTrue();
    }

    [Test]
    public async Task InitialState_AllCars_NotCrashed()
    {
        var sim = BuildSim(new FullThrottleController(), new IdleController());
        await Assert.That(sim.State.Cars.All(c => !c.Crashed)).IsTrue();
    }

    [Test]
    public async Task InitialState_AllCars_NearStartPose()
    {
        var sim = BuildSim(new IdleController());
        var car = sim.State.Cars[0];
        var startX = sim.Track.StartPose.Position.X;
        var startY = sim.Track.StartPose.Position.Y;
        await Assert.That(car.Position.X).IsEqualTo(startX).Within(5f);
        await Assert.That(car.Position.Y).IsEqualTo(startY).Within(5f);
    }

    [Test]
    public async Task InitialState_GateIndex_IsNumGatesMinusOne()
    {
        var sim = BuildSim(new IdleController());
        int numGates = _track.Gates.Count;
        await Assert.That(sim.State.Cars[0].CurrentGateIndex).IsEqualTo(numGates - 1);
    }

    [Test]
    public async Task InitialState_TicksAlive_IsZero()
    {
        var sim = BuildSim(new IdleController());
        await Assert.That(sim.State.Cars.All(c => c.TicksAlive == 0)).IsTrue();
    }

    [Test]
    public async Task InitialState_CompletedLaps_IsZero()
    {
        var sim = BuildSim(new FullThrottleController());
        await Assert.That(sim.State.Cars[0].CompletedLaps).IsEqualTo(0);
    }

    [Test]
    public async Task Tick_WithRewardModel_AccumulatesTrainingReward()
    {
        var rewardSum = new double[1];
        var sim = new RaceSimulation(_track, [new FullThrottleController()], new DefaultRewardModel(), rewardSum);
        sim.Reset();
        for (var i = 0; i < 400; i++)
            sim.Tick();
        await Assert.That(rewardSum[0]).IsGreaterThan(0);
    }

    [Test]
    public async Task Tick_AdvancesTickCounter()
    {
        var sim = BuildSim(new IdleController());
        sim.Tick();
        await Assert.That(sim.State.Tick).IsEqualTo(1);
    }

    [Test]
    public async Task Tick_Multiple_TickCounterAccumulates()
    {
        var sim = BuildSim(new IdleController());
        for (int i = 0; i < 10; i++) sim.Tick();
        await Assert.That(sim.State.Tick).IsEqualTo(10);
    }

    [Test]
    public async Task Tick_FullThrottle_CarGainsSpeed()
    {
        var sim = BuildSim(new FullThrottleController());
        sim.Tick();
        await Assert.That(sim.State.Cars[0].Speed).IsGreaterThan(0);
    }

    [Test]
    public async Task Tick_IdleController_SpeedRemainsZero()
    {
        var sim = BuildSim(new IdleController());
        for (int i = 0; i < 10; i++) sim.Tick();
        await Assert.That(sim.State.Cars[0].Speed).IsEqualTo(0.0).Within(0.001);
    }

    [Test]
    public async Task Tick_FullThrottle_After20Ticks_SpeedIsPositive()
    {
        var sim = BuildSim(new FullThrottleController());
        for (int i = 0; i < 20; i++) sim.Tick();
        await Assert.That(sim.State.Cars[0].Speed).IsGreaterThan(0);
    }

    [Test]
    public async Task Tick_FullThrottle_SpeedDoesNotExceedMaxSpeed()
    {
        var sim = BuildSim(new FullThrottleController());
        double max = 0;
        for (int i = 0; i < 200; i++)
        {
            sim.Tick();
            var speed = sim.State.Cars[0].Speed;
            if (speed > max) max = speed;
        }
        await Assert.That(max).IsLessThanOrEqualTo(12.0 + 0.001);
    }

    [Test]
    public async Task Tick_FullThrottle_PositionChanges()
    {
        var sim = BuildSim(new FullThrottleController());
        var start = sim.State.Cars[0].Position;
        sim.Tick();
        for (int i = 0; i < 5; i++) sim.Tick();
        await Assert.That(sim.State.Cars[0].Position).IsNotEqualTo(start);
    }

    [Test]
    public async Task Tick_CrashedCar_SpeedIsZero()
    {
        var sim = BuildSim(new FullThrottleController());
        sim.State.Cars[0].Position = new Vector2(-5, -5);
        sim.Tick();
        await Assert.That(sim.State.Cars[0].Crashed).IsTrue();
        await Assert.That(sim.State.Cars[0].Speed).IsEqualTo(0);
    }

    [Test]
    public async Task Tick_CrashedCar_PositionDoesNotChange()
    {
        var sim = BuildSim(new FullThrottleController());
        sim.State.Cars[0].Position = new Vector2(-5, -5);
        sim.Tick();
        var crashPos = sim.State.Cars[0].Position;
        for (int i = 0; i < 20; i++) sim.Tick();
        await Assert.That(sim.State.Cars[0].Position).IsEqualTo(crashPos);
    }

    [Test]
    public async Task Tick_CarOutsideBounds_IsCrashed()
    {
        var sim = BuildSim(new IdleController());
        sim.State.Cars[0].Position = new Vector2(_track.Width + 10, _track.Height + 10);
        sim.Tick();
        await Assert.That(sim.State.Cars[0].Crashed).IsTrue();
    }

    [Test]
    public async Task Tick_TicksAlive_IncreasesWhenNotCrashed()
    {
        var sim = BuildSim(new SlowLeftController());
        int initialAlive = 0;
        for (int i = 0; i < 5; i++)
        {
            sim.Tick();
            if (!sim.State.Cars[0].Crashed && sim.State.Cars[0].TicksAlive > initialAlive)
                initialAlive = sim.State.Cars[0].TicksAlive;
        }
        await Assert.That(initialAlive).IsGreaterThan(0);
    }

    [Test]
    public async Task Tick_Fitness_IsNonNegative()
    {
        var sim = BuildSim(new FullThrottleController());
        for (int i = 0; i < 30; i++) sim.Tick();
        await Assert.That(sim.State.Cars[0].Fitness).IsGreaterThanOrEqualTo(0.0);
    }

    [Test]
    public async Task Reset_TickGoesBackToZero()
    {
        var sim = BuildSim(new FullThrottleController());
        for (int i = 0; i < 20; i++) sim.Tick();
        sim.Reset();
        await Assert.That(sim.State.Tick).IsEqualTo(0);
    }

    [Test]
    public async Task Reset_SpeedGoesBackToZero()
    {
        var sim = BuildSim(new FullThrottleController());
        for (int i = 0; i < 20; i++) sim.Tick();
        sim.Reset();
        await Assert.That(sim.State.Cars[0].Speed).IsEqualTo(0.0).Within(0.001);
    }

    [Test]
    public async Task Reset_CrashedCar_IsNoLongerCrashed()
    {
        var sim = BuildSim(new FullThrottleController());
        sim.State.Cars[0].Position = new Vector2(-5, -5);
        sim.Tick();
        await Assert.That(sim.State.Cars[0].Crashed).IsTrue();
        sim.Reset();
        await Assert.That(sim.State.Cars[0].Crashed).IsFalse();
    }

    [Test]
    public async Task Reset_CompletedLaps_GoesBackToZero()
    {
        var sim = BuildSim(new FullThrottleController());
        sim.State.Cars[0].CompletedLaps = 3;
        sim.Reset();
        await Assert.That(sim.State.Cars[0].CompletedLaps).IsEqualTo(0);
    }

    [Test]
    public async Task Reset_MultipleCycles_EachTimeTickIsZero()
    {
        var sim = BuildSim(new IdleController());
        for (int cycle = 0; cycle < 3; cycle++)
        {
            for (int t = 0; t < 5; t++) sim.Tick();
            sim.Reset();
            await Assert.That(sim.State.Tick).IsEqualTo(0).Because($"after reset cycle {cycle}");
        }
    }

    [Test]
    public async Task MultipleControllers_CarsCount_MatchesControllerCount()
    {
        var sim = BuildSim(new IdleController(), new FullThrottleController(), new LeftTurnController());
        await Assert.That(sim.State.Cars.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Leaderboard_InitialStandings_Count_MatchesControllers()
    {
        var sim = BuildSim(new IdleController(), new FullThrottleController());
        await Assert.That(sim.State.Leaderboard.Standings.Count).IsEqualTo(2);
    }

    [Test]
    public async Task Leaderboard_AfterTicks_Count_StillMatchesControllers()
    {
        var sim = BuildSim(new IdleController(), new FullThrottleController(), new LeftTurnController());
        for (int i = 0; i < 10; i++) sim.Tick();
        await Assert.That(sim.State.Leaderboard.Standings.Count).IsEqualTo(3);
    }

    [Test]
    public async Task Leaderboard_StandingsAreOrdered_ByPosition()
    {
        var sim = BuildSim(new FullThrottleController(), new IdleController(), new LeftTurnController());
        for (int i = 0; i < 30; i++) sim.Tick();
        var positions = sim.State.Leaderboard.Standings.Select(s => s.Position).ToArray();
        await Assert.That(positions).IsInOrder();
    }

    [Test]
    public async Task Track_Property_ReturnsSameTrackPassedToConstructor()
    {
        var sim = BuildSim(new IdleController());
        await Assert.That(sim.Track).IsSameReferenceAs(_track);
    }

    [Test]
    public async Task Controllers_Property_ReturnsSameListPassedToConstructor()
    {
        IRaceCarController[] controllers = [new IdleController(), new FullThrottleController()];
        var sim = new RaceSimulation(_track, controllers);
        await Assert.That(sim.Controllers.Count).IsEqualTo(2);
    }
}
