namespace Novolis.Simulation.SpaceCombat;

public enum CraftRole
{
    Freighter,
    Fighter,
    Hostile,
}

public sealed class CraftProfile
{
    public string Id { get; init; } = "";
    public CraftRole Role { get; init; }
    public float MaxSpeed { get; init; } = 48f;
    public float MinSpeed { get; init; } = 6f;
    public float Acceleration { get; init; } = 28f;
    public float Deceleration { get; init; } = 22f;
    public float Drag { get; init; } = 0.35f;
    public float TurnRate { get; init; } = 2.2f;
    public float HitRadius { get; init; } = 2.5f;
    public float MaxShield { get; init; } = 1f;
    public float MaxHull { get; init; } = 1f;
    public string? MeshId { get; init; }

    public static CraftProfile FighterDefault => new()
    {
        Id = "default_fighter",
        Role = CraftRole.Fighter,
        MaxSpeed = 48f,
        MinSpeed = 6f,
        Acceleration = 28f,
        Deceleration = 22f,
        Drag = 0.35f,
        TurnRate = 2.2f,
        HitRadius = 2.6f,
        MaxShield = 1f,
        MaxHull = 1f,
    };

    public static CraftProfile FreighterDefault => new()
    {
        Id = "default_freighter",
        Role = CraftRole.Freighter,
        MaxSpeed = 18f,
        MinSpeed = 4f,
        Acceleration = 10f,
        Deceleration = 12f,
        Drag = 0.45f,
        TurnRate = 0.9f,
        HitRadius = 6.5f,
        MaxShield = 1.4f,
        MaxHull = 1.2f,
    };

    public static CraftProfile HostileDefault => new()
    {
        Id = "default_hostile",
        Role = CraftRole.Hostile,
        MaxSpeed = 42f,
        MinSpeed = 8f,
        Acceleration = 24f,
        Deceleration = 20f,
        Drag = 0.3f,
        TurnRate = 2.4f,
        HitRadius = 2.4f,
        MaxShield = 0.35f,
        MaxHull = 0.7f,
    };
}
