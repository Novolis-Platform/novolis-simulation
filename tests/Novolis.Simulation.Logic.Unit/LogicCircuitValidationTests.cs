using Novolis.Simulation.Logic;

namespace Novolis.Simulation.Logic.Unit;

public sealed class LogicCircuitValidationTests
{
    [Test]
    public async Task Two_drivers_on_one_input_are_rejected()
    {
        var first = LogicNode.CreateInput("A", value: LogicValue.Low);
        var second = LogicNode.CreateInput("B", value: LogicValue.High);
        var gate = LogicNode.CreateGate(LogicNodeKind.And);
        var circuit = new LogicCircuit(
            [first, second, gate],
            [
                new LogicConnection(
                    LogicConnectionId.New(),
                    new LogicPortAddress(first.Id, "out"),
                    new LogicPortAddress(gate.Id, "a")),
                new LogicConnection(
                    LogicConnectionId.New(),
                    new LogicPortAddress(second.Id, "out"),
                    new LogicPortAddress(gate.Id, "a")),
            ]);

        var result = circuit.Validate();

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Diagnostics.Any(diagnostic =>
                diagnostic.Code == LogicDiagnosticCode.InputCardinalityExceeded))
            .IsTrue();
    }

    [Test]
    public async Task Feedback_cycle_is_reported_as_non_convergent()
    {
        var first = LogicNode.CreateGate(LogicNodeKind.Not);
        var second = LogicNode.CreateGate(LogicNodeKind.Not);
        var circuit = new LogicCircuit(
            [first, second],
            [
                new LogicConnection(
                    LogicConnectionId.New(),
                    new LogicPortAddress(first.Id, "out"),
                    new LogicPortAddress(second.Id, "in")),
                new LogicConnection(
                    LogicConnectionId.New(),
                    new LogicPortAddress(second.Id, "out"),
                    new LogicPortAddress(first.Id, "in")),
            ]);

        var result = circuit.Evaluate();

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Diagnostics.Any(diagnostic =>
                diagnostic.Code == LogicDiagnosticCode.Cycle))
            .IsTrue();
        await Assert.That(result.Diagnostics.Any(diagnostic =>
                diagnostic.Code == LogicDiagnosticCode.NonConvergent))
            .IsTrue();
    }

    [Test]
    public async Task Try_connect_rejects_an_input_as_the_source()
    {
        var input = LogicNode.CreateInput();
        var output = LogicNode.CreateOutput();
        var circuit = LogicCircuit.Empty.AddNode(input).AddNode(output);

        var result = circuit.TryConnect(
            LogicConnectionId.New(),
            new LogicPortAddress(input.Id, "out"),
            new LogicPortAddress(input.Id, "out"));

        await Assert.That(result.IsValid).IsFalse();
        await Assert.That(result.Diagnostics.Any(diagnostic =>
                diagnostic.Code == LogicDiagnosticCode.DestinationIsNotInput))
            .IsTrue();
    }

    [Test]
    public async Task Validation_is_stable_for_the_same_circuit()
    {
        var input = LogicNode.CreateInput();
        var output = LogicNode.CreateOutput();
        var circuit = LogicCircuit.Empty
            .AddNode(input)
            .AddNode(output)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(input.Id, "out"),
                new LogicPortAddress(output.Id, "in"));

        var first = circuit.Validate();
        var second = circuit.Validate();

        await Assert.That(first.Diagnostics.Select(diagnostic => diagnostic.Message))
            .IsEquivalentTo(second.Diagnostics.Select(diagnostic => diagnostic.Message));
    }
}
