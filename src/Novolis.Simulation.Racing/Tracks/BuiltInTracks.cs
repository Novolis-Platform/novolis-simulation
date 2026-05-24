namespace Novolis.Simulation.Racing.Tracks;

/// <summary>Registered layouts for simulations, evolution, and tooling.</summary>
public static class BuiltInTracks
{
    // —— Original full-grid circuits (120×50 class) ——————————————————————————

    /// <summary>Circle.</summary>
    public static readonly ITrackDefinition Circle = new CircleTrack();
    /// <summary>Oval.</summary>
    public static readonly ITrackDefinition Oval = new OvalTrack();
    /// <summary>Stadium.</summary>
    public static readonly ITrackDefinition Stadium = new StadiumTrack();
    /// <summary>Club.</summary>
    public static readonly ITrackDefinition Club = new ClubTrack();
    /// <summary>Chicane.</summary>
    public static readonly ITrackDefinition Chicane = new ChicaneTrack();

    // —— Compact & polyline “reasonable” set (smaller rasters, varied geometry) —

    /// <summary>MicroCircle.</summary>
    public static readonly ITrackDefinition MicroCircle = new MicroCircleTrack();
    /// <summary>CompactOval.</summary>
    public static readonly ITrackDefinition CompactOval = new CompactOvalTrack();
    /// <summary>ShortChicane.</summary>
    public static readonly ITrackDefinition ShortChicane = new ShortChicaneTrack();
    /// <summary>EssesCircuit.</summary>
    public static readonly ITrackDefinition EssesCircuit = new EssesCircuitTrack();
    /// <summary>MountainPass.</summary>
    public static readonly ITrackDefinition MountainPass = new MountainPassTrack();
    /// <summary>KartIndoor.</summary>
    public static readonly ITrackDefinition KartIndoor = new KartIndoorTrack();
    /// <summary>RiverSprint.</summary>
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

    /// <summary>ById operation.</summary>
    public static ITrackDefinition ById(string id) =>
        All.FirstOrDefault(t => string.Equals(t.Id, id, StringComparison.OrdinalIgnoreCase)) ?? Circle;
}
