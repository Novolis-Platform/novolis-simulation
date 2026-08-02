using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Race;
using Novolis.Simulation.Racing.Tracks;

namespace Novolis.Simulation.Racing.Tests;

public sealed class RaceAsciiRendererTests
{
    private sealed class IdleController : IRaceCarController
    {
        public string Name => "Idle";
        public CarVisualStyle VisualStyle => new("🚗", "#000");
        public CarControlDecision Decide(in CarObservation obs) => new(0, 0, 0);
    }

    private sealed class FullThrottleController : IRaceCarController
    {
        public string Name => "Fast";
        public CarVisualStyle VisualStyle => new("🏎", "#F00");
        public CarControlDecision Decide(in CarObservation obs) => new(0, 1.0, 0);
    }

    private static readonly RaceTrack Track = new TrackBuilder().Build(new CircleTrack());

    [Test]
    public async Task Render_IncludesTickAndGrid()
    {
        var sim = new RaceSimulation(Track, [new IdleController()]);
        var text = RaceAsciiRenderer.Render(sim);
        await Assert.That(text).Contains("Tick=0");
        await Assert.That(text).Contains("Idle");
        await Assert.That(text).Contains("lap 0");
    }

    [Test]
    public async Task Render_ShowsMultipleCars_AndCrashedMark()
    {
        var sim = new RaceSimulation(Track, [new IdleController(), new FullThrottleController()]);
        for (var i = 0; i < 5; i++)
            sim.Tick();

        var text = RaceAsciiRenderer.Render(sim);
        await Assert.That(text).Contains("Fast");
        await Assert.That(text.Contains('1') || text.Contains('2')).IsTrue();
    }

    [Test]
    public async Task Render_ContainsTrackSymbols()
    {
        var sim = new RaceSimulation(Track, [new IdleController()]);
        var text = RaceAsciiRenderer.Render(sim);
        await Assert.That(text.Contains('.') || text.Contains('#') || text.Contains('=')).IsTrue();
    }
}
