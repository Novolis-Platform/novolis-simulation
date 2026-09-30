using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

/// <summary>Produces a partial or full <see cref="FlightIntent"/> for a craft.</summary>
public interface IFlightController
{
    FlightIntent Tick(in CraftObservation observation);
}
