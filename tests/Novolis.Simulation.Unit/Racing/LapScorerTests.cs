namespace Novolis.Simulation.Racing.Tests;

using System.Numerics;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Tracks;

using TUnit.Assertions;

public sealed class LapScorerTests
{
    private static RaceTrack BuildMinimalTrack(IReadOnlyList<TrackGate> gates)
    {
        const int w = 100, h = 60;
        var cells = new TrackCell[w, h];
        for (int c = 0; c < w; c++)
            for (int r = 0; r < h; r++)
                cells[c, r] = TrackCell.Road;

        Vector2[] samples = [new(50, 5), new(95, 30), new(50, 55), new(5, 30)];
        Vector2[] tangents = [Vector2.UnitX, Vector2.UnitY, -Vector2.UnitX, -Vector2.UnitY];
        double[] arcLens = [0, 50, 100, 150];

        var progressMap = new TrackProgressMap
        {
            Samples = samples,
            Tangents = tangents,
            CumulativeArcLengths = arcLens,
            TotalArcLength = 200
        };

        var geometry = new TrackGeometry
        {
            LeftBoundary = Array.Empty<Vector2>(),
            RightBoundary = Array.Empty<Vector2>(),
            HalfWidth = 5.0
        };

        return new RaceTrack
        {
            Id = "test",
            Name = "Test",
            Width = w,
            Height = h,
            Cells = cells,
            CenterLineSamples = samples,
            Gates = gates,
            StartPose = new TrackStartPose(new Vector2(50, 30), Vector2.UnitX),
            ProgressMap = progressMap,
            Geometry = geometry
        };
    }

    private static readonly IReadOnlyList<TrackGate> ThreeGates =
    [
        new TrackGate(0, new Vector2(50, 20), new Vector2(50, 30), 0.0),
        new TrackGate(1, new Vector2(70, 20), new Vector2(70, 30), 0.33),
        new TrackGate(2, new Vector2(30, 20), new Vector2(30, 30), 0.66),
    ];

    private static readonly RaceTrack ThreeGateTrack = BuildMinimalTrack(ThreeGates);
    private static readonly RaceTrack EmptyGateTrack = BuildMinimalTrack([]);

    private static CarState MakeCar(int currentGateIndex = 2) => new()
    {
        Id = 0,
        Name = "TestCar",
        Position = new Vector2(50, 25),
        PreviousPosition = new Vector2(50, 25),
        Forward = Vector2.UnitX,
        Speed = 0,
        SteeringAngle = 0,
        Crashed = false,
        WrongWay = false,
        CompletedLaps = 0,
        CurrentGateIndex = currentGateIndex,
        TrackProgress = 0,
        Fitness = 0,
        TicksAlive = 0,
        WrongWayTicks = 0
    };

    private static void CrossGate(CarState car, TrackGate gate)
    {
        var mid = (gate.A + gate.B) / 2f;
        var normal = Vector2.Normalize(new Vector2(gate.B.Y - gate.A.Y, -(gate.B.X - gate.A.X)));
        car.PreviousPosition = mid - normal * 2;
        car.Position = mid + normal * 2;
    }

    private readonly LapScorer _scorer = new();

    [Test]
    public async Task Update_EmptyTrack_DoesNotChangeLaps()
    {
        var car = MakeCar(0);
        car.PreviousPosition = new Vector2(10, 25);
        car.Position = new Vector2(20, 25);
        _scorer.Update(EmptyGateTrack, car);
        await Assert.That(car.CompletedLaps).IsEqualTo(0);
    }

    [Test]
    public async Task Update_EmptyTrack_DoesNotChangeGateIndex()
    {
        var car = MakeCar(0);
        car.PreviousPosition = new Vector2(10, 25);
        car.Position = new Vector2(20, 25);
        _scorer.Update(EmptyGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task InitialGateIndex_ShouldBeNumGatesMinusOne()
    {
        int numGates = ThreeGateTrack.Gates.Count;
        int expectedInitialIndex = numGates - 1;
        await Assert.That(expectedInitialIndex).IsEqualTo(2);
    }

    [Test]
    public async Task Update_NoCrossing_GateIndexUnchanged()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(10, 25);
        car.Position = new Vector2(20, 25);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(2);
    }

    [Test]
    public async Task Update_NoCrossing_LapsUnchanged()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(10, 25);
        car.Position = new Vector2(20, 25);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CompletedLaps).IsEqualTo(0);
    }

    [Test]
    public async Task Update_CrossGate0_FromLastGate_IncreasesLaps()
    {
        var car = MakeCar(2);
        CrossGate(car, ThreeGates[0]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CompletedLaps).IsEqualTo(1);
    }

    [Test]
    public async Task Update_CrossGate0_FromLastGate_SetsCurrentGateTo0()
    {
        var car = MakeCar(2);
        CrossGate(car, ThreeGates[0]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task Update_CrossGate1_AfterGate0_AdvancesGateIndex()
    {
        var car = MakeCar(0);
        CrossGate(car, ThreeGates[1]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(1);
    }

    [Test]
    public async Task Update_CrossGate2_AfterGate1_AdvancesGateIndex()
    {
        var car = MakeCar(1);
        CrossGate(car, ThreeGates[2]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(2);
    }

    [Test]
    public async Task Update_CrossGate1_NoLapIncrement()
    {
        var car = MakeCar(0);
        CrossGate(car, ThreeGates[1]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CompletedLaps).IsEqualTo(0);
    }

    [Test]
    public async Task Update_CrossGate2_WhenExpecting1_NoChange()
    {
        var car = MakeCar(0);
        CrossGate(car, ThreeGates[2]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
        await Assert.That(car.CompletedLaps).IsEqualTo(0);
    }

    [Test]
    public async Task Update_CrossGate1_WhenExpecting0_NoChange()
    {
        var car = MakeCar(2);
        CrossGate(car, ThreeGates[1]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(2);
        await Assert.That(car.CompletedLaps).IsEqualTo(0);
    }

    [Test]
    public async Task Update_FullLap_LapCountIs1()
    {
        var car = MakeCar(2);

        CrossGate(car, ThreeGates[0]);
        _scorer.Update(ThreeGateTrack, car);

        CrossGate(car, ThreeGates[1]);
        _scorer.Update(ThreeGateTrack, car);

        CrossGate(car, ThreeGates[2]);
        _scorer.Update(ThreeGateTrack, car);

        await Assert.That(car.CompletedLaps).IsEqualTo(1);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(2);
    }

    [Test]
    public async Task Update_TwoFullLaps_LapCountIs2()
    {
        var car = MakeCar(2);

        for (int lap = 0; lap < 2; lap++)
        {
            CrossGate(car, ThreeGates[0]);
            _scorer.Update(ThreeGateTrack, car);

            CrossGate(car, ThreeGates[1]);
            _scorer.Update(ThreeGateTrack, car);

            CrossGate(car, ThreeGates[2]);
            _scorer.Update(ThreeGateTrack, car);
        }

        await Assert.That(car.CompletedLaps).IsEqualTo(2);
    }

    [Test]
    public async Task Update_ParallelPath_DoesNotRegister()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(51, 20);
        car.Position = new Vector2(51, 30);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(2);
    }

    [Test]
    public async Task Update_PerpendicularCrossing_Registers()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(46, 25);
        car.Position = new Vector2(54, 25);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task Update_ExactMidpointCrossing_Registers()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(48, 25);
        car.Position = new Vector2(52, 25);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task Update_ReversedCrossing_StillRegisters()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(52, 25);
        car.Position = new Vector2(48, 25);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task Update_BarelyClipsGateEndpoint_Registers()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(48, 20);
        car.Position = new Vector2(52, 20);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task Update_PathDoesNotReachGate_DoesNotRegister()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(48, 10);
        car.Position = new Vector2(52, 10);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(2);
    }

    [Test]
    public async Task Update_SingleGate_CrossingAlwaysIncreasesLaps()
    {
        var singleGate = new List<TrackGate>
        {
            new TrackGate(0, new Vector2(50, 20), new Vector2(50, 30), 0.0)
        };
        var track = BuildMinimalTrack(singleGate);
        var car = MakeCar(0);

        car.PreviousPosition = new Vector2(48, 25);
        car.Position = new Vector2(52, 25);
        _scorer.Update(track, car);

        await Assert.That(car.CompletedLaps).IsEqualTo(1);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task Update_SingleGate_SecondCrossing_IncreasesLapAgain()
    {
        var singleGate = new List<TrackGate>
        {
            new TrackGate(0, new Vector2(50, 20), new Vector2(50, 30), 0.0)
        };
        var track = BuildMinimalTrack(singleGate);
        var car = MakeCar(0);

        for (int i = 0; i < 3; i++)
        {
            car.PreviousPosition = new Vector2(48, 25);
            car.Position = new Vector2(52, 25);
            _scorer.Update(track, car);
        }

        await Assert.That(car.CompletedLaps).IsEqualTo(3);
    }

    [Test]
    public async Task Update_CrossGate0_WhenCurrentGateIsNot_LastGate_NoLapIncrement()
    {
        var car = MakeCar(1);
        CrossGate(car, ThreeGates[0]);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CompletedLaps).IsEqualTo(0);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(1);
    }

    [Test]
    public async Task Update_SameGateCalledTwice_SecondCallDoesNotAdvanceAgain()
    {
        var car = MakeCar(2);

        CrossGate(car, ThreeGates[0]);
        _scorer.Update(ThreeGateTrack, car);

        CrossGate(car, ThreeGates[0]);
        _scorer.Update(ThreeGateTrack, car);

        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }

    [Test]
    public async Task Update_TwoGates_BothCrossedSequentially_BothAdvance()
    {
        var twoGates = new List<TrackGate>
        {
            new TrackGate(0, new Vector2(50, 20), new Vector2(50, 30), 0.0),
            new TrackGate(1, new Vector2(70, 20), new Vector2(70, 30), 0.5),
        };
        var track = BuildMinimalTrack(twoGates);
        var car = MakeCar(1);

        car.PreviousPosition = new Vector2(48, 25);
        car.Position = new Vector2(52, 25);
        _scorer.Update(track, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);

        car.PreviousPosition = new Vector2(68, 25);
        car.Position = new Vector2(72, 25);
        _scorer.Update(track, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(1);
    }

    [Test]
    public async Task Update_CrossGateFromBelow_Registers()
    {
        var car = MakeCar(2);
        car.PreviousPosition = new Vector2(50 - 2, 25);
        car.Position = new Vector2(50 + 2, 28);
        _scorer.Update(ThreeGateTrack, car);
        await Assert.That(car.CurrentGateIndex).IsEqualTo(0);
    }
}
