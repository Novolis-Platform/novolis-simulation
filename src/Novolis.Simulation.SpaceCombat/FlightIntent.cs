namespace Novolis.Simulation.SpaceCombat;

/// <summary>Frame input for arcade flight (host fills from keyboard/mouse/pad).</summary>
public struct FlightIntent
{
    public float YawDelta;
    public float PitchDelta;
    public float RollLeft;
    public float RollRight;
    public float ThrottleUp;
    public float ThrottleDown;
    public bool Fire;
    public bool Transfer;
}
