<!-- novolis-pkg-brand:start -->
[![Novolis](https://raw.githubusercontent.com/Novolis-Platform/.github/main/brand/logo-icon.png)](https://novolis-platform.github.io/.github/novolis-simulation/)

[Novolis](https://github.com/Novolis-Platform) · [Docs](https://novolis-platform.github.io/.github/novolis-simulation/) · [Source](https://github.com/Novolis-Platform/novolis-simulation)
<!-- novolis-pkg-brand:end -->

# Novolis.Simulation.Logic

Framework-neutral, deterministic digital logic circuits for Novolis applications and labs.

## Install

```bash
dotnet add package Novolis.Simulation.Logic
```

## Quick start

```csharp
using Novolis.Simulation.Logic;

var input = LogicNode.CreateInput("A");
var output = LogicNode.CreateOutput("Result");
var circuit = LogicCircuit.Empty
    .AddNode(input)
    .AddNode(output)
    .Connect(
        LogicConnectionId.New(),
        new LogicPortAddress(input.Id, "out"),
        new LogicPortAddress(output.Id, "in"));

var result = circuit
    .SetInputValue(input.Id, LogicValue.High)
    .Evaluate();
var value = result.OutputValues[output.Id];
```

The evaluator is intentionally deterministic. Invalid wiring, feedback cycles,
missing custom-chip definitions, and unresolved signal states are reported as
diagnostics instead of being resolved through timing or random traversal.
