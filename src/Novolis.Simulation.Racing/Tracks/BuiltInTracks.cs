namespace Novolis.Simulation.Racing.Tracks;

/// <summary>Registered layouts for simulations, evolution, and tooling.</summary>
public static class BuiltInTracks
{
    // —— Original full-grid circuits (120×50 class) ——————————————————————————

    public static readonly ITrackDefinition Circle = new CircleTrack();
    public static readonly ITrackDefinition Oval = new OvalTrack();
    public static readonly ITrackDefinition Stadium = new StadiumTrack();
    public static readonly ITrackDefinition Club = new ClubTrack();
    public static readonly ITrackDefinition Chicane = new ChicaneTrack();

    // —— Compact & polyline “reasonable” set (smaller rasters, varied geometry) —

    public static readonly ITrackDefinition MicroCircle = new MicroCircleTrack();
    public static readonly ITrackDefinition CompactOval = new CompactOvalTrack();
    public static readonly ITrackDefinition ShortChicane = new ShortChicaneTrack();
    public static readonly ITrackDefinition EssesCircuit = new EssesCircuitTrack();
    public static readonly ITrackDefinition MountainPass = new MountainPassTrack();
    public static readonly ITrackDefinition KartIndoor = new KartIndoorTrack();
    public static readonly ITrackDefinition RiverSprint = new RiverSprintTrack();

    /// <summary>Every registered layout (integration tests iterate this list).</summary>
    public static IReadOnlyList<ITrackDefinition> All =>
    [
        Circle, Oval, Stadium, Club, Chicane,
        MicroCircle, CompactOval, ShortChicane, EssesCircuit,
        MountainPass, KartIndoor, RiverSprint
    ];

    /// <summary>
    /// Curated subset: compact rasters and technical polylines, plus symmetric circle baseline.
    /// Omits the largest duplicate-feel ovals when you want faster builds or UI previews.
    /// </summary>
    public static IReadOnlyList<ITrackDefinition> Reasonable =>
    [
        MicroCircle, KartIndoor, CompactOval,
        ShortChicane, EssesCircuit, MountainPass, RiverSprint,
        Club, Chicane, Circle
    ];

    public static ITrackDefinition ById(string id) =>
        All.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase)) ?? Circle;
}
