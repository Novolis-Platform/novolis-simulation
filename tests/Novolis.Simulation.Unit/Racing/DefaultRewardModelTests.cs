namespace Novolis.Simulation.Racing.Tests;

using System.Numerics;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Rewards;
using Novolis.Simulation.Racing.Tracks;

using TUnit.Assertions;

public sealed class DefaultRewardModelTests
{
    private static readonly RaceTrack _track = new TrackBuilder().Build(new Novolis.Simulation.Racing.Tracks.CircleTrack());
    private readonly DefaultRewardModel _model = new();

    private static CarState MakeCar(
        double speed = 0.0,
        bool crashed = false,
        bool wrongWay = false,
        int completedLaps = 0,
        int currentGate = 0) => new()
        {
            Id = 0,
            Name = "TestCar",
            Position = _track.StartPose.Position,
            PreviousPosition = _track.StartPose.Position,
            Forward = _track.StartPose.Forward,
            Speed = speed,
            SteeringAngle = 0,
            Crashed = crashed,
            WrongWay = wrongWay,
            CompletedLaps = completedLaps,
            CurrentGateIndex = currentGate,
            TrackProgress = 0,
            Fitness = 0,
            TicksAlive = 0,
            WrongWayTicks = 0
        };

    private static TrackProgressSample Progress(
        double loopT = 0.0,
        double alignment = 0.5,
        double centerOffset = 0.0,
        bool isWrongWay = false) =>
        new(loopT, loopT, alignment, centerOffset, isWrongWay);

    [Test]
    public async Task Evaluate_ForwardProgress_Positive_ReturnsPositiveReward()
    {
        var prev = Progress(loopT: 0.0);
        var curr = Progress(loopT: 0.1);
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), prev, curr);
        await Assert.That(result.ForwardProgressReward).IsGreaterThan(0);
    }

    [Test]
    public async Task Evaluate_ForwardProgress_Delta01_Returns1()
    {
        var prev = Progress(loopT: 0.0);
        var curr = Progress(loopT: 0.1);
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), prev, curr);
        await Assert.That(result.ForwardProgressReward).IsEqualTo(1.0).Within(0.001);
    }

    [Test]
    public async Task Evaluate_BackwardProgress_NoReward()
    {
        var prev = Progress(loopT: 0.5);
        var curr = Progress(loopT: 0.3);
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), prev, curr);
        await Assert.That(result.ForwardProgressReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_WrapAround_LoopTJumpsFrom095To005_ReturnsPositiveReward()
    {
        var prev = Progress(loopT: 0.95);
        var curr = Progress(loopT: 0.05);
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), prev, curr);
        await Assert.That(result.ForwardProgressReward).IsEqualTo(1.0).Within(0.001);
    }

    [Test]
    public async Task Evaluate_NoProgress_ForwardProgressReward_IsZero()
    {
        var prog = Progress(loopT: 0.5);
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), prog, prog);
        await Assert.That(result.ForwardProgressReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_SmallWrapAround_NotTreatedAsWrap()
    {
        var prev = Progress(loopT: 0.8);
        var curr = Progress(loopT: 0.3);
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), prev, curr);
        await Assert.That(result.ForwardProgressReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_Speed5_Returns05()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(speed: 5.0), Progress(), Progress());
        await Assert.That(result.SpeedReward).IsEqualTo(0.5).Within(0.001);
    }

    [Test]
    public async Task Evaluate_SpeedAboveThreshold_NonZeroReward()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(speed: 1.0), Progress(), Progress());
        await Assert.That(result.SpeedReward).IsGreaterThan(0);
    }

    [Test]
    public async Task Evaluate_SpeedBelowThreshold03_ZeroSpeedReward()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(speed: 0.3), Progress(), Progress());
        await Assert.That(result.SpeedReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_ZeroSpeed_ZeroSpeedReward()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(speed: 0.0), Progress(), Progress());
        await Assert.That(result.SpeedReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_SpeedExactlyAtThreshold05_ZeroSpeedReward()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(speed: 0.5), Progress(), Progress());
        await Assert.That(result.SpeedReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_Crashed_CrashPenalty5()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(crashed: true), Progress(), Progress());
        await Assert.That(result.CrashPenalty).IsEqualTo(5.0);
    }

    [Test]
    public async Task Evaluate_NotCrashed_CrashPenalty0()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(crashed: false), Progress(), Progress());
        await Assert.That(result.CrashPenalty).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_WrongWay_Penalty05()
    {
        var currProgress = Progress(isWrongWay: true);
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(wrongWay: true), Progress(), currProgress);
        await Assert.That(result.WrongWayPenalty).IsEqualTo(0.5);
    }

    [Test]
    public async Task Evaluate_NotWrongWay_Penalty0()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), Progress(), Progress());
        await Assert.That(result.WrongWayPenalty).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_GateAdvanced_GateReward2()
    {
        var prev = MakeCar(currentGate: 0);
        var curr = MakeCar(currentGate: 1);
        var result = _model.Evaluate(_track, prev, curr, Progress(), Progress());
        await Assert.That(result.GateReward).IsEqualTo(2.0);
    }

    [Test]
    public async Task Evaluate_SameGateIndex_NoGateReward()
    {
        var prev = MakeCar(currentGate: 1);
        var curr = MakeCar(currentGate: 1);
        var result = _model.Evaluate(_track, prev, curr, Progress(), Progress());
        await Assert.That(result.GateReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_GateRegressedNotAdvanced_NoGateReward()
    {
        var prev = MakeCar(currentGate: 3);
        var curr = MakeCar(currentGate: 1);
        var result = _model.Evaluate(_track, prev, curr, Progress(), Progress());
        await Assert.That(result.GateReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_LapCompleted_LapReward10()
    {
        var prev = MakeCar(completedLaps: 0);
        var curr = MakeCar(completedLaps: 1);
        var result = _model.Evaluate(_track, prev, curr, Progress(), Progress());
        await Assert.That(result.LapReward).IsEqualTo(10.0);
    }

    [Test]
    public async Task Evaluate_SameLaps_NoLapReward()
    {
        var prev = MakeCar(completedLaps: 1);
        var curr = MakeCar(completedLaps: 1);
        var result = _model.Evaluate(_track, prev, curr, Progress(), Progress());
        await Assert.That(result.LapReward).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_WallProximityPenalty_IsAlwaysZero()
    {
        var result = _model.Evaluate(_track, MakeCar(), MakeCar(), Progress(), Progress());
        await Assert.That(result.WallProximityPenalty).IsEqualTo(0.0);
    }

    [Test]
    public async Task Evaluate_Total_MatchesSumOfComponents()
    {
        var prev = MakeCar(completedLaps: 0, currentGate: 0);
        var curr = MakeCar(speed: 3.0, completedLaps: 1, currentGate: 1);
        var prevP = Progress(loopT: 0.0);
        var currP = Progress(loopT: 0.1);
        var r = _model.Evaluate(_track, prev, curr, prevP, currP);

        double expected = r.ForwardProgressReward + r.SpeedReward
                          - r.WallProximityPenalty - r.WrongWayPenalty - r.CrashPenalty
                          + r.GateReward + r.LapReward;

        await Assert.That(r.Total).IsEqualTo(expected).Within(0.001);
    }

    [Test]
    public async Task Evaluate_AllPositive_TotalGreaterThan10()
    {
        var prev = MakeCar(completedLaps: 0, currentGate: 0);
        var curr = MakeCar(speed: 5.0, completedLaps: 1, currentGate: 1);
        var prevP = Progress(loopT: 0.0);
        var currP = Progress(loopT: 0.1);
        var r = _model.Evaluate(_track, prev, curr, prevP, currP);
        await Assert.That(r.Total).IsGreaterThan(10.0);
    }

    [Test]
    public async Task Evaluate_CrashedAndWrongWay_TotalIsNegative()
    {
        var curr = MakeCar(crashed: true, wrongWay: true, speed: 0);
        var currP = Progress(loopT: 0.0, isWrongWay: true);
        var r = _model.Evaluate(_track, MakeCar(), curr, Progress(loopT: 0.0), currP);
        await Assert.That(r.Total).IsLessThan(0.0);
    }

    [Test]
    public async Task Evaluate_ZeroBaseline_NoChanges_TotalIsZero()
    {
        var car = MakeCar(speed: 0, crashed: false, wrongWay: false);
        var prog = Progress(loopT: 0.0, isWrongWay: false);
        var r = _model.Evaluate(_track, car, car, prog, prog);
        await Assert.That(r.Total).IsEqualTo(0.0).Within(0.001);
    }
}
