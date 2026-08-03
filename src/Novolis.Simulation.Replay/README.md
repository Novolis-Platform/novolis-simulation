<!-- novolis-pkg-brand:start -->
<p align="center">
  <a href="https://github.com/Novolis-Platform/novolis-simulation">
    <img src="https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.svg" width="72" alt="Novolis"/>
  </a>
</p>
<!-- novolis-pkg-brand:end -->

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

## API

| Type | Role |
|------|------|
| `SimulationTimeline<TState>` | `Initial` + ordered `Steps` |
| `SimulationStepRecord<TState>` | Step metadata and end state |
| `InMemorySimulationRecorder<TState>` | `SetInitial`, `RecordStep`, `Build` |
| `ReplayPlayback` | `GetEndStateAt`, `VerifyStep`, `VerifyAllSteps` |
| `SimultaneousPlanBuffer<TPlan>` | WEGO: `Submit`, `PendingPlans`, `Clear` |
| `ISimulationStepRunner<TState>` | Deterministic step runner for verification |
| `ISimulationEventStore<TEvent>` | Append/read event log |
| `InMemorySimulationEventStore<TEvent>` | In-process event store |

## Related

| Package | When to use |
|---------|-------------|
| `Novolis.Simulation` | `SimulationClock` for fixed-step ticks |
| `Novolis.Simulation.Abstractions` | `SimulationStep`, `ISimulationSystem` |

Product event-sourced games keep domain events in the app; use this package for **snapshot/step** replay and tests.

