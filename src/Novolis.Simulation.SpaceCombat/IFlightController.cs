using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

/// <summary>Snapshot fed to flight / gunner controllers each tick.</summary>
public readonly struct CraftObservation
{
    public required CraftState Self { get; init; }
    public required Vector3? TargetPosition { get; init; }
    public required Vector3? TargetVelocity { get; init; }
    public required Vector3 EscortAnchor { get; init; }
    public required int ActiveThreats { get; init; }
    public required float Dt { get; init; }

    public static CraftObservation FromSession(
        MissionSession session,
        CraftState self,
        float dt,
        CraftState? preferredTarget = null)
    {
        var target = preferredTarget ?? Targeting.FindLockTarget(
            session.Hostiles, self.Position, self.Forward);
        return new CraftObservation
        {
            Self = self,
            TargetPosition = target?.Position,
            TargetVelocity = target?.Velocity,
            EscortAnchor = session.Freighter.Position,
            ActiveThreats = session.ActiveHostiles,
            Dt = dt,
        };
    }
}

/// <summary>Produces a partial or full <see cref="FlightIntent"/> for a craft.</summary>
public interface IFlightController
{
    FlightIntent Tick(in CraftObservation observation);
}
