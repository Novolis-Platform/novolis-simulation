namespace Novolis.Simulation.Racing.Cars;

using System.Numerics;

/// <summary>Represents CarState.</summary>
public sealed class CarState
{
    /// <summary>Id.</summary>
    public required int Id { get; init; }
    /// <summary>Name.</summary>
    public required string Name { get; init; }

    /// <summary>Position.</summary>
    public required Vector2 Position { get; set; }
    /// <summary>PreviousPosition.</summary>
    public required Vector2 PreviousPosition { get; set; }
    /// <summary>Forward.</summary>
    public required Vector2 Forward { get; set; }

    /// <summary>Speed.</summary>
    public required double Speed { get; set; }
    /// <summary>SteeringAngle.</summary>
    public required double SteeringAngle { get; set; }

    /// <summary>Crashed.</summary>
    public required bool Crashed { get; set; }
    /// <summary>WrongWay.</summary>
    public required bool WrongWay { get; set; }
    /// <summary>CompletedLaps.</summary>
    public required int CompletedLaps { get; set; }

    /// <summary>CurrentGateIndex.</summary>
    public required int CurrentGateIndex { get; set; }
    /// <summary>TrackProgress.</summary>
    public required double TrackProgress { get; set; }
    /// <summary>Fitness.</summary>
    public required double Fitness { get; set; }

    /// <summary>TicksAlive.</summary>
    public required int TicksAlive { get; set; }
    /// <summary>WrongWayTicks.</summary>
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
