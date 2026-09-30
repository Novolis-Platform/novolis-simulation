using System.Numerics;

namespace Novolis.Simulation.View;

/// <summary>Active character camera mode.</summary>
public enum CharacterCameraMode
{
    /// <summary>First-person eye camera.</summary>
    FirstPerson = 0,

    /// <summary>Third-person boom camera.</summary>
    ThirdPerson = 1,

    /// <summary>Orbit rig around the character.</summary>
    Orbit = 2
}
