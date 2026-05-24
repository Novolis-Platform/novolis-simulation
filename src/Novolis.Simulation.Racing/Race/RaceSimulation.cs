namespace Novolis.Simulation.Racing.Race;

using System.Numerics;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Rewards;
using Novolis.Simulation.Racing.Sensors;
using Novolis.Simulation.Racing.Tracks;

public sealed class RaceSimulation : IRaceSimulation
{
    private const double DeltaTime = 1.0 / 60.0;
    private const double MaxSpeed = 12.0;
    private const double MaxTurnRateRadiansPerSecond = 2.0;
    private const double Acceleration = 8.0;

    private readonly ICarSensorModel _sensorModel = new DefaultCarSensorModel();
    private readonly ILapScorer _lapScorer = new LapScorer();
    private readonly ITrackProgressResolver _progressResolver = new TrackProgressResolver();
    private readonly IRewardModel? _trainingRewardModel;
    private readonly double[]? _trainingRewardAccumulator;

    public RaceTrack Track { get; }
    public IReadOnlyList<IRaceCarController> Controllers { get; }
    public RaceWorldState State { get; private set; }

    public RaceSimulation(
        RaceTrack track,
        IReadOnlyList<IRaceCarController> controllers,
        IRewardModel? trainingRewardModel = null,
        double[]? trainingRewardAccumulator = null)
    {
        Track = track;
        Controllers = controllers;
        _trainingRewardModel = trainingRewardModel;
        _trainingRewardAccumulator = trainingRewardAccumulator;
        if (trainingRewardModel is not null)
        {
            if (trainingRewardAccumulator is null)
                throw new ArgumentNullException(nameof(trainingRewardAccumulator), "Accumulator is required when a training reward model is set.");
            if (trainingRewardAccumulator.Length != controllers.Count)
                throw new ArgumentException("Training reward accumulator length must match controller count.", nameof(trainingRewardAccumulator));
        }
        else if (trainingRewardAccumulator is not null)
            throw new ArgumentException("Training reward model is required when an accumulator is provided.", nameof(trainingRewardModel));

        State = CreateInitialState();
    }

    public void Reset()
    {
        State = CreateInitialState();
    }

    public void Tick()
    {
        State.Tick++;
        var cars = State.Cars;

        for (int index = 0; index < cars.Count; index++)
        {
            var car = cars[index];
            if (car.Crashed) continue;

            var carBeforeTick = car.CloneForComparison();
            var progressBefore = _progressResolver.Resolve(Track, car.Position, car.Forward);

            var sensors = _sensorModel.Read(Track, car);
            var standing = GetStanding(car, State);
            var observation = new CarObservation(index, car, sensors, progressBefore, standing);

            var controller = Controllers[index];
            var decision = controller.Decide(in observation);

            car.PreviousPosition = car.Position;

            car.Speed += (decision.Throttle - decision.Brake) * Acceleration * DeltaTime;
            car.Speed = Math.Clamp(car.Speed, 0, MaxSpeed);

            double turnRadians = decision.Steering * MaxTurnRateRadiansPerSecond * DeltaTime;
            double cos = Math.Cos(turnRadians);
            double sin = Math.Sin(turnRadians);
            float newFx = (float)(car.Forward.X * cos - car.Forward.Y * sin);
            float newFy = (float)(car.Forward.X * sin + car.Forward.Y * cos);
            car.Forward = Vector2.Normalize(new Vector2(newFx, newFy));
            car.SteeringAngle = decision.Steering;

            car.Position += car.Forward * (float)(car.Speed * DeltaTime);

            int col = (int)car.Position.X;
            int row = (int)car.Position.Y;

            if (col < 0 || col >= Track.Width || row < 0 || row >= Track.Height)
            {
                car.Crashed = true;
            }
            else if (Track.Cells[col, row] == TrackCell.Wall || Track.Cells[col, row] == TrackCell.Empty)
            {
                car.Crashed = true;
            }

            if (car.Crashed)
            {
                car.Speed = 0;
                var progressAfterCrash = _progressResolver.Resolve(Track, car.Position, car.Forward);
                AccumulateTrainingReward(index, carBeforeTick, car, progressBefore, progressAfterCrash);
                continue;
            }

            _lapScorer.Update(Track, car);

            var updatedProgress = _progressResolver.Resolve(Track, car.Position, car.Forward);
            car.WrongWay = updatedProgress.IsWrongWay;
            if (car.WrongWay) car.WrongWayTicks++;
            car.TrackProgress = updatedProgress.LoopT;
            car.TicksAlive++;

            car.Fitness = car.CompletedLaps + car.TrackProgress + car.Speed * 0.01;
            AccumulateTrainingReward(index, carBeforeTick, car, progressBefore, updatedProgress);
        }

        State.Leaderboard = BuildLeaderboard(cars);
    }

    private void AccumulateTrainingReward(
        int carIndex,
        CarState previous,
        CarState current,
        TrackProgressSample previousProgress,
        TrackProgressSample currentProgress)
    {
        if (_trainingRewardModel is null || _trainingRewardAccumulator is null)
            return;
        var breakdown = _trainingRewardModel.Evaluate(Track, previous, current, previousProgress, currentProgress);
        _trainingRewardAccumulator[carIndex] += breakdown.Total;
    }

    private RaceWorldState CreateInitialState()
    {
        int numGates = Track.Gates.Count;
        var cars = Controllers.Select((ctrl, i) => new CarState
        {
            Id = i,
            Name = ctrl.Name,
            Position = Track.StartPose.Position + new Vector2(i * 1.5f, 0),
            PreviousPosition = Track.StartPose.Position + new Vector2(i * 1.5f, 0),
            Forward = Track.StartPose.Forward,
            Speed = 0,
            SteeringAngle = 0,
            Crashed = false,
            WrongWay = false,
            CompletedLaps = 0,
            CurrentGateIndex = numGates > 0 ? numGates - 1 : 0,
            TrackProgress = 0,
            Fitness = 0,
            TicksAlive = 0,
            WrongWayTicks = 0
        }).ToArray();

        var leaderboard = BuildLeaderboard(cars);

        return new RaceWorldState
        {
            Tick = 0,
            Cars = cars,
            Leaderboard = leaderboard,
            IsFinished = false
        };
    }

    private static RaceLeaderboard BuildLeaderboard(IReadOnlyList<CarState> cars)
    {
        var standings = cars
            .OrderByDescending(c => c.CompletedLaps)
            .ThenByDescending(c => c.TrackProgress)
            .Select((c, pos) => new RaceStanding(pos + 1, c.Id, c.Name, c.CompletedLaps, c.TrackProgress))
            .ToArray();
        return new RaceLeaderboard { Standings = standings };
    }

    private static RaceStanding GetStanding(CarState car, RaceWorldState state)
    {
        var sorted = state.Cars
            .OrderByDescending(c => c.CompletedLaps)
            .ThenByDescending(c => c.TrackProgress)
            .ToList();
        int pos = sorted.IndexOf(car) + 1;
        return new RaceStanding(pos, car.Id, car.Name, car.CompletedLaps, car.TrackProgress);
    }
}
