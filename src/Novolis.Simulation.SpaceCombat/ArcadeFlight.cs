namespace Novolis.Simulation.SpaceCombat;

public static class ArcadeFlight
{
    public static void Apply(CraftState craft, in FlightIntent intent, float dt)
    {
        if (!craft.Active)
            return;

        var p = craft.Profile;
        craft.Yaw += intent.YawDelta;
        craft.Pitch = Math.Clamp(craft.Pitch + intent.PitchDelta, -1.1f, 1.1f);

        if (intent.RollLeft > 0)
            craft.Roll = Math.Min(craft.Roll + 2.8f * p.TurnRate * 0.5f * intent.RollLeft * dt, 0.75f);
        if (intent.RollRight > 0)
            craft.Roll = Math.Max(craft.Roll - 2.8f * p.TurnRate * 0.5f * intent.RollRight * dt, -0.75f);
        craft.Roll *= 1f - 3.5f * dt;

        if (intent.ThrottleUp > 0)
            craft.Speed = Math.Min(craft.Speed + p.Acceleration * intent.ThrottleUp * dt, p.MaxSpeed);
        if (intent.ThrottleDown > 0)
            craft.Speed = Math.Max(craft.Speed - p.Deceleration * intent.ThrottleDown * dt, p.MinSpeed);

        craft.Speed *= 1f - p.Drag * dt;
        if (craft.Speed < p.MinSpeed)
            craft.Speed = p.MinSpeed;

        var forward = craft.Forward;
        craft.Velocity = forward * craft.Speed;
        craft.Position += craft.Velocity * dt;
        craft.FireCooldown = Math.Max(0, craft.FireCooldown - dt);
    }
}
