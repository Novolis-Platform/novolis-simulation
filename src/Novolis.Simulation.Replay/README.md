# Novolis.Simulation.Replay

Deterministic **tick timelines**, step integrity checks, and a small **WEGO plan buffer** (collect plans → commit phase).

## Install

```bash
dotnet add package Novolis.Simulation.Replay
```

## Quick start

```csharp
using Novolis.Simulation;
using Novolis.Simulation.Replay;

var clock = new SimulationClock();
var recorder = new InMemorySimulationRecorder<MyState>();
recorder.SetInitial(initial);

var state = initial;
foreach (var _ in Enumerable.Range(0, 10))
{
    var step = clock.Advance();
    state = Step(state, step);
    recorder.RecordStep(step, state, stepSeed: (int)step.Tick);
}

var timeline = recorder.Build();
var ok = ReplayPlayback.VerifyAllSteps(timeline, myRunner);
```

## Related

- Product event-sourced games (e.g. Star Conflicts Revolt) keep domain events in the app; use this package for **snapshot/step** replay and tests.
- [fleetcommander-patterns-for-platform.md](https://github.com/Novolis-Platform/novolis-governance/blob/main/docs/imports-todo/fleetcommander-patterns-for-platform.md)
