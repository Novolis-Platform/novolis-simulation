namespace Novolis.Simulation.Racing.Cars;

using System.Numerics;

public sealed class CarState
{
    public required int Id { get; init; }
    public required string Name { get; init; }

    public required Vector2 Position { get; set; }
    public required Vector2 PreviousPosition { get; set; }
    public required Vector2 Forward { get; set; }

    public required double Speed { get; set; }
    public required double SteeringAngle { get; set; }

    public required bool Crashed { get; set; }
    public required bool WrongWay { get; set; }
    public required int CompletedLaps { get; set; }

    public required int CurrentGateIndex { get; set; }
    public required double TrackProgress { get; set; }
    public required double Fitness { get; set; }

    public required int TicksAlive { get; set; }
    public required int WrongWayTicks { get; set; }

    /// <summary>Shallow copy of all fields for <c>IRewardModel</c> comparisons between simulation ticks.</summary>
    public CarState CloneForComparison() =>
        new()
        {
            Id = Id,
            Name = Name,
            Position = Position,
            PreviousPosition = PreviousPosition,
            Forward = Forward,
            Speed = Speed,
            SteeringAngle = SteeringAngle,
            Crashed = Crashed,
            WrongWay = WrongWay,
            CompletedLaps = CompletedLaps,
            CurrentGateIndex = CurrentGateIndex,
            TrackProgress = TrackProgress,
            Fitness = Fitness,
            TicksAlive = TicksAlive,
            WrongWayTicks = WrongWayTicks
        };
}
