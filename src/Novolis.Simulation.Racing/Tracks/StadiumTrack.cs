namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

/// <summary>Represents StadiumTrack.</summary>
public sealed class StadiumTrack : ITrackDefinition
{
    /// <summary>Id.</summary>
    public string Id => "stadium";
    /// <summary>Name.</summary>
    public string Name => "Stadium";
    /// <summary>BuildSpec.</summary>
    public TrackBuildSpec BuildSpec { get; } = new(
        120, 50, 4.0, 1.0, 5,
        new SplineLoop(CreatePoints()),
        Enumerable.Range(0, 14).Select(i => i / 14.0).ToArray(),
        0.0);

    private static IReadOnlyList<Vector2> CreatePoints()
    {
        var pts = new List<Vector2>();
        for (int i = 0; i <= 4; i++) pts.Add(new Vector2(25 + i * 12.5f, 38));
        for (int i = 1; i <= 4; i++)
        {
            var a = MathF.PI * 1.5f + MathF.PI * i / 4f;
            pts.Add(new Vector2(85 + MathF.Cos(a) * 10, 25 + MathF.Sin(a) * 13));
        }
        for (int i = 0; i <= 4; i++) pts.Add(new Vector2(75 - i * 12.5f, 12));
        for (int i = 1; i <= 4; i++)
        {
            var a = MathF.PI * 0.5f + MathF.PI * i / 4f;
            pts.Add(new Vector2(25 + MathF.Cos(a) * 10, 25 + MathF.Sin(a) * 13));
        }
        return pts;
    }
}
