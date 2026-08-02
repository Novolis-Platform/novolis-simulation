using Novolis.Simulation;
using Novolis.Simulation.Abstractions;

namespace Novolis.Simulation.Unit;

public sealed class SimulationClockTests
{
    [Test]
    public async Task Default_ctor_uses_sixtieth_second_step()
    {
        var clock = new SimulationClock();
        await Assert.That(clock.FixedDeltaSeconds).IsEqualTo(1.0 / 60.0).Within(1e-12);
    }

    [Test]
    public async Task Advance_increments_tick_and_returns_step()
    {
        var clock = new SimulationClock(0.25);
        var step = clock.Advance();

        await Assert.That(clock.Tick).IsEqualTo(1ul);
        await Assert.That(step.Tick).IsEqualTo(1ul);
        await Assert.That(step.DeltaSeconds).IsEqualTo(0.25);
        await Assert.That(clock.ElapsedSeconds).IsEqualTo(0.25);
    }

    [Test]
    public async Task Reset_clears_tick_and_elapsed()
    {
        var clock = new SimulationClock(0.1);
        clock.Advance();
        clock.Advance();
        clock.Reset();

        await Assert.That(clock.Tick).IsEqualTo(0ul);
        await Assert.That(clock.ElapsedSeconds).IsEqualTo(0);
    }

    [Test]
    public async Task Non_positive_delta_throws()
    {
        await Assert.That(() => new SimulationClock(0)).Throws<ArgumentOutOfRangeException>();
    }
}
