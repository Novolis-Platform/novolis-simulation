namespace Novolis.Simulation.SpaceCombat;

public enum MissionPhase
{
    Freighter,
    Transfer,
    Fighter,
    Complete,
    Failed,
}

public sealed class MissionDescriptor
{
    public string Id { get; init; } = "";
    public required CraftProfile FreighterProfile { get; init; }
    public required CraftProfile FighterProfile { get; init; }
    public required CraftProfile HostileProfile { get; init; }
    public int HostileCount { get; init; } = 6;
    public float ProtectSeconds { get; init; } = 40f;
    public int DestroyRequired { get; init; } = 4;
    public int MaxHostilesAlive { get; init; } = 10;
}
