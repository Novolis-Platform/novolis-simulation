using Novolis.Simulation.Logic;

namespace Novolis.Simulation.Logic.Unit;

public sealed class LogicCircuitEvaluationTests
{
    [Test]
    public async Task And_gate_produces_high_only_when_both_inputs_are_high()
    {
        var fixture = BuildBinaryCircuit(LogicNodeKind.And, LogicValue.High, LogicValue.High);

        var result = fixture.Circuit.Evaluate();

        await Assert.That(result.IsValid).IsTrue();
        await Assert.That(result.OutputValues[fixture.Output.Id])
            .IsEqualTo(LogicValue.High);
    }

    [Test]
    public async Task Or_gate_produces_high_when_either_input_is_high()
    {
        var fixture = BuildBinaryCircuit(LogicNodeKind.Or, LogicValue.Low, LogicValue.High);

        var result = fixture.Circuit.Evaluate();

        await Assert.That(result.OutputValues[fixture.Output.Id])
            .IsEqualTo(LogicValue.High);
    }

    [Test]
    public async Task Xor_gate_produces_low_for_equal_inputs()
    {
        var fixture = BuildBinaryCircuit(LogicNodeKind.Xor, LogicValue.High, LogicValue.High);

        var result = fixture.Circuit.Evaluate();

        await Assert.That(result.OutputValues[fixture.Output.Id])
            .IsEqualTo(LogicValue.Low);
    }

    [Test]
    public async Task Not_and_nand_invert_resolved_values()
    {
        var input = LogicNode.CreateInput("A", value: LogicValue.High);
        var not = LogicNode.CreateGate(LogicNodeKind.Not);
        var nand = LogicNode.CreateGate(LogicNodeKind.Nand);
        var output = LogicNode.CreateOutput("Result");
        var circuit = LogicCircuit.Empty
            .AddNode(input)
            .AddNode(not)
            .AddNode(nand)
            .AddNode(output)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(input.Id, "out"),
                new LogicPortAddress(not.Id, "in"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(input.Id, "out"),
                new LogicPortAddress(nand.Id, "a"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(input.Id, "out"),
                new LogicPortAddress(nand.Id, "b"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(nand.Id, "out"),
                new LogicPortAddress(output.Id, "in"));

        var result = circuit.Evaluate();

        await Assert.That(result.GetValue(new LogicPortAddress(not.Id, "out")))
            .IsEqualTo(LogicValue.Low);
        await Assert.That(result.OutputValues[output.Id])
            .IsEqualTo(LogicValue.Low);
    }

    [Test]
    public async Task Disconnected_inputs_produce_unknown_for_ambiguous_gate_state()
    {
        var fixture = BuildBinaryCircuit(
            LogicNodeKind.And,
            LogicValue.High,
            LogicValue.Disconnected);

        var result = fixture.Circuit.Evaluate();

        await Assert.That(result.OutputValues[fixture.Output.Id])
            .IsEqualTo(LogicValue.Unknown);
    }

    [Test]
    public async Task Fan_out_can_drive_multiple_outputs()
    {
        var input = LogicNode.CreateInput("A", value: LogicValue.High);
        var outputA = LogicNode.CreateOutput("Result A");
        var outputB = LogicNode.CreateOutput("Result B");
        var circuit = LogicCircuit.Empty
            .AddNode(input)
            .AddNode(outputA)
            .AddNode(outputB)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(input.Id, "out"),
                new LogicPortAddress(outputA.Id, "in"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(input.Id, "out"),
                new LogicPortAddress(outputB.Id, "in"));

        var result = circuit.Evaluate();

        await Assert.That(result.OutputValues[outputA.Id]).IsEqualTo(LogicValue.High);
        await Assert.That(result.OutputValues[outputB.Id]).IsEqualTo(LogicValue.High);
    }

    private static BinaryFixture BuildBinaryCircuit(
        LogicNodeKind kind,
        LogicValue left,
        LogicValue right)
    {
        var inputA = LogicNode.CreateInput("A", value: left);
        var inputB = LogicNode.CreateInput("B", value: right);
        var gate = LogicNode.CreateGate(kind);
        var output = LogicNode.CreateOutput("Result");
        var circuit = LogicCircuit.Empty
            .AddNode(inputA)
            .AddNode(inputB)
            .AddNode(gate)
            .AddNode(output)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(inputA.Id, "out"),
                new LogicPortAddress(gate.Id, "a"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(inputB.Id, "out"),
                new LogicPortAddress(gate.Id, "b"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(gate.Id, "out"),
                new LogicPortAddress(output.Id, "in"));
        return new BinaryFixture(circuit, inputA, inputB, gate, output);
    }

    private sealed record BinaryFixture(
        LogicCircuit Circuit,
        LogicNode InputA,
        LogicNode InputB,
        LogicNode Gate,
        LogicNode Output);
}
