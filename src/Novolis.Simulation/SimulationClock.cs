namespace Novolis.Simulation;

using Novolis.Simulation.Abstractions;

/// <summary>Advances simulation time and produces <see cref="SimulationStep"/> values.</summary>
public sealed class SimulationClock
{
    private ulong _tick;

    public SimulationClock(double fixedDeltaSeconds = 1.0 / 60.0)
    {
        if (fixedDeltaSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(fixedDeltaSeconds));

        FixedDeltaSeconds = fixedDeltaSeconds;
    }

    public double FixedDeltaSeconds { get; }
    public double ElapsedSeconds => _tick * FixedDeltaSeconds;
    public ulong Tick => _tick;

    public SimulationStep Advance() =>
        new(FixedDeltaSeconds, ++_tick);

    public void Reset()
    {
        _tick = 0;
    }
}
