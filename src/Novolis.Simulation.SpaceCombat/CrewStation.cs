namespace Novolis.Simulation.SpaceCombat;

/// <summary>
/// Who the human crew member is manning.
/// <see cref="Pilot"/> → AI handles gunnery; <see cref="Gunner"/> → AI handles flight.
/// </summary>
public enum CrewStation
{
    /// <summary>Human does both flight and fire (legacy / solo).</summary>
    Dual = 0,

    /// <summary>Human flies; AI aims/fires.</summary>
    Pilot = 1,

    /// <summary>Human guns (aim + fire); AI pilots the craft.</summary>
    Gunner = 2,
}
