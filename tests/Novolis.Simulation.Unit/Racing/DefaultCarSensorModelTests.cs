namespace Novolis.Simulation.Racing.Tests;

using System.Numerics;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Sensors;
using Novolis.Simulation.Racing.Tracks;

using TUnit.Assertions;

public sealed class DefaultCarSensorModelTests
{
    private static readonly RaceTrack _track = new TrackBuilder().Build(new Novolis.Simulation.Racing.Tracks.CircleTrack());
    private readonly DefaultCarSensorModel _model = new();

    private static CarState MakeCar(
        Vector2? position = null,
        Vector2? forward = null,
        double speed = 0.0) => new()
        {
            Id = 0,
            Name = "Sensor",
            Position = position ?? _track.StartPose.Position,
            PreviousPosition = position ?? _track.StartPose.Position,
            Forward = forward ?? _track.StartPose.Forward,
            Speed = speed,
            SteeringAngle = 0,
            Crashed = false,
            WrongWay = false,
            CompletedLaps = 0,
            CurrentGateIndex = 0,
            TrackProgress = 0,
            Fitness = 0,
            TicksAlive = 0,
            WrongWayTicks = 0
        };

    [Test]
    public async Task Definitions_Count_Is10()
    {
        await Assert.That(_model.Definitions.Count).IsEqualTo(10);
    }

    [Test]
    public async Task Definitions_First7_AreWallSensors()
    {
        var names = _model.Definitions.Take(7).Select(d => d.Name).ToArray();
        await Assert.That(names).Contains("WallLeft90");
        await Assert.That(names).Contains("WallLeft45");
        await Assert.That(names).Contains("WallLeft20");
        await Assert.That(names).Contains("WallAhead");
        await Assert.That(names).Contains("WallRight20");
        await Assert.That(names).Contains("WallRight45");
        await Assert.That(names).Contains("WallRight90");
    }

    [Test]
    public async Task Definitions_Index7_IsSpeed()
    {
        await Assert.That(_model.Definitions[7].Name).IsEqualTo("Speed");
    }

    [Test]
    public async Task Definitions_Index8_IsAlignment()
    {
        await Assert.That(_model.Definitions[8].Name).IsEqualTo("Alignment");
    }

    [Test]
    public async Task Definitions_Index9_IsCenterOffset()
    {
        await Assert.That(_model.Definitions[9].Name).IsEqualTo("CenterOffset");
    }

    [Test]
    public async Task Definitions_WallLeft90_HasMaxRange15()
    {
        await Assert.That(_model.Definitions[0].MaxRange).IsEqualTo(15);
    }

    [Test]
    public async Task Definitions_WallAhead_HasMaxRange30()
    {
        await Assert.That(_model.Definitions[3].MaxRange).IsEqualTo(30);
    }

    [Test]
    public async Task Read_ValuesLength_Is10()
    {
        var car = MakeCar();
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values.Length).IsEqualTo(10);
    }

    [Test]
    public async Task Read_HitsLength_Is10()
    {
        var car = MakeCar();
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Hits.Length).IsEqualTo(10);
    }

    [Test]
    public async Task Read_AllValues_InZeroToOneRange()
    {
        var car = MakeCar();
        var reading = _model.Read(_track, car);
        foreach (var v in reading.Values)
            await Assert.That(v).IsBetween(0.0, 1.0);
    }

    [Test]
    public async Task Read_SpeedSensor_ZeroSpeed_ReturnsZero()
    {
        var car = MakeCar(speed: 0.0);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[7]).IsEqualTo(0.0).Within(0.001);
    }

    [Test]
    public async Task Read_SpeedSensor_MaxSpeed10_ReturnsOne()
    {
        var car = MakeCar(speed: 10.0);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[7]).IsEqualTo(1.0).Within(0.001);
    }

    [Test]
    public async Task Read_SpeedSensor_HalfMaxSpeed_ReturnsHalf()
    {
        var car = MakeCar(speed: 5.0);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[7]).IsEqualTo(0.5).Within(0.001);
    }

    [Test]
    public async Task Read_SpeedSensor_AboveMax_IsClamped()
    {
        var car = MakeCar(speed: 15.0);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[7]).IsEqualTo(1.0).Within(0.001);
    }

    [Test]
    public async Task Read_SpeedSensor_NegativeSpeed_IsClampedToZero()
    {
        var car = MakeCar(speed: -5.0);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[7]).IsEqualTo(0.0).Within(0.001);
    }

    [Test]
    public async Task Read_AlignmentSensor_ForwardAlongTrack_IsHighValue()
    {
        var car = MakeCar(forward: _track.StartPose.Forward);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[8]).IsGreaterThan(0.7);
    }

    [Test]
    public async Task Read_AlignmentSensor_FacingBackward_IsLowValue()
    {
        var backward = Vector2.Normalize(-_track.StartPose.Forward);
        var car = MakeCar(forward: backward);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[8]).IsLessThan(0.3);
    }

    [Test]
    public async Task Read_AlignmentSensor_IsInZeroToOneRange()
    {
        var car = MakeCar();
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[8]).IsBetween(0.0, 1.0);
    }

    [Test]
    public async Task Read_CenterOffsetSensor_IsInZeroToOneRange()
    {
        var car = MakeCar();
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[9]).IsBetween(0.0, 1.0);
    }

    [Test]
    public async Task Read_WallSensors_AllInZeroToOneRange()
    {
        var car = MakeCar();
        var reading = _model.Read(_track, car);
        for (int i = 0; i < 7; i++)
            await Assert.That(reading.Values[i]).IsBetween(0.0, 1.0)
                .Because($"wall sensor {i} should be in [0,1]");
    }

    [Test]
    public async Task Read_WallAheadSensor_WhenFacingWall_IsHigh()
    {
        var pos = new Vector2(89, 25);
        var forward = Vector2.UnitX;
        var car = MakeCar(position: pos, forward: forward);
        var reading = _model.Read(_track, car);
        await Assert.That(reading.Values[3]).IsGreaterThan(0.3);
    }

    [Test]
    public async Task Read_HitNames_MatchDefinitionNames()
    {
        var car = MakeCar();
        var reading = _model.Read(_track, car);
        for (int i = 0; i < 10; i++)
            await Assert.That(reading.Hits[i].Name).IsEqualTo(_model.Definitions[i].Name);
    }

    [Test]
    public async Task Read_SameCarSamePosition_ProducesSameValues()
    {
        var car = MakeCar(speed: 3.0);
        var reading1 = _model.Read(_track, car);
        var reading2 = _model.Read(_track, car);
        await Assert.That(reading1.Values.ToArray()).IsEquivalentTo(reading2.Values.ToArray());
    }

    [Test]
    public async Task Read_TwoDifferentCars_ProduceDifferentSpeedValues()
    {
        var car1 = MakeCar(speed: 0.0);
        var car2 = MakeCar(speed: 5.0);

        var r1 = _model.Read(_track, car1);
        var r2 = _model.Read(_track, car2);

        await Assert.That(System.Math.Abs(r1.Values[7] - r2.Values[7])).IsGreaterThan(0.01);
    }

    [Test]
    public async Task Read_CarAtTrackCorner_DoesNotThrow()
    {
        var car = MakeCar(position: new Vector2(1, 1));
        await Assert.That(() => _model.Read(_track, car)).ThrowsNothing();
    }

    [Test]
    public async Task Read_CarNearTrackBoundary_DoesNotThrow()
    {
        var car = MakeCar(position: new Vector2(_track.Width - 2, _track.Height - 2));
        await Assert.That(() => _model.Read(_track, car)).ThrowsNothing();
    }
}
