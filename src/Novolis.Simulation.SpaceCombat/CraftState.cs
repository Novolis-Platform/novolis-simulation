using System.Numerics;

namespace Novolis.Simulation.SpaceCombat;

public sealed class CraftState
{
    public required CraftProfile Profile { get; init; }
    public Vector3 Position;
    public float Yaw;
    public float Pitch;
    public float Roll;
    public float Speed = 22f;
    public float Shield;
    public float Hull;
    public bool Active = true;
    public bool PlayerControlled;
    public float WeavePhase;
    public float FireCooldown;
    public Vector3 Velocity;

    public Vector3 Forward
    {
        get
        {
            var cp = MathF.Cos(Pitch);
            return Vector3.Normalize(new Vector3(
                MathF.Sin(Yaw) * cp,
                MathF.Sin(Pitch),
                MathF.Cos(Yaw) * cp));
        }
    }

    public float Throttle01
    {
        get
        {
            var span = Profile.MaxSpeed - Profile.MinSpeed;
            return span <= 1e-4f ? 0f : Math.Clamp((Speed - Profile.MinSpeed) / span, 0f, 1f);
        }
    }

    public void ResetVitals()
    {
        Shield = Profile.MaxShield;
        Hull = Profile.MaxHull;
        Active = true;
    }
}
