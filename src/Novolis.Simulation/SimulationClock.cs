namespace Novolis.Simulation;

using Novolis.Simulation.Abstractions;

/// <summary>Advances simulation time and produces <see cref="SimulationStep"/> values.</summary>
public sealed class SimulationClock
{
    private ulong _tick;

    /// <summary>SimulationClock operation.</summary>
    public SimulationClock(double fixedDeltaSeconds = 1.0 / 60.0)
    {
        if (fixedDeltaSeconds <= 0)
            throw new ArgumentOutOfRangeException(nameof(fixedDeltaSeconds));

        FixedDeltaSeconds = fixedDeltaSeconds;
    }

    /// <summary>FixedDeltaSeconds.</summary>
    public double FixedDeltaSeconds { get; }
    /// <summary>ElapsedSeconds.</summary>
    public double ElapsedSeconds => _tick * FixedDeltaSeconds;
    /// <summary>Tick.</summary>
    public ulong Tick => _tick;

    /// <summary>Advance operation.</summary>
    public SimulationStep Advance() =>
        new(FixedDeltaSeconds, ++_tick);

    /// <summary>Reset operation.</summary>
    public void Reset()
    {
        _tick = 0;
    }
}
