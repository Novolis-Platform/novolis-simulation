using Novolis.Simulation.Logic;

namespace Novolis.Simulation.Logic.Unit;

public sealed class LogicCircuitHistoryAndPersistenceTests
{
    [Test]
    public async Task History_can_undo_and_redo_immutable_edits()
    {
        var history = new LogicCircuitHistory();
        var input = LogicNode.CreateInput();

        history.Execute(circuit => circuit.AddNode(input));
        await Assert.That(history.Current.Nodes.Count).IsEqualTo(1);
        await Assert.That(history.CanUndo).IsTrue();

        await Assert.That(history.Undo()).IsTrue();
        await Assert.That(history.Current.Nodes.Count).IsEqualTo(0);
        await Assert.That(history.CanRedo).IsTrue();

        await Assert.That(history.Redo()).IsTrue();
        await Assert.That(history.Current.Nodes.Count).IsEqualTo(1);
    }

    [Test]
    public async Task Circuit_json_round_trip_preserves_identity_and_configuration()
    {
        var input = LogicNode.CreateInput(
            "A",
            x: 4,
            z: 8,
            value: LogicValue.High);
        var output = LogicNode.CreateOutput("Result", x: 12, z: 8);
        var circuit = LogicCircuit.Empty
            .AddNode(input)
            .AddNode(output)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(input.Id, "out"),
                new LogicPortAddress(output.Id, "in"));

        var restored = LogicCircuitJson.Deserialize(
            LogicCircuitJson.Serialize(circuit));
        var result = restored.Evaluate();

        await Assert.That(restored.Nodes.Count).IsEqualTo(2);
        await Assert.That(restored.Connections.Count).IsEqualTo(1);
        await Assert.That(restored.Nodes.Single(node => node.Id == input.Id).X)
            .IsEqualTo(4);
        await Assert.That(result.OutputValues[output.Id])
            .IsEqualTo(LogicValue.High);
    }

    [Test]
    public async Task Custom_chip_can_be_saved_reused_and_round_tripped()
    {
        var (library, topLevel, output) = BuildCustomAndCircuit();

        var result = topLevel.Evaluate(library);
        var restoredLibrary = LogicChipLibraryJson.Deserialize(
            LogicChipLibraryJson.Serialize(library));
        var restoredResult = topLevel.Evaluate(restoredLibrary);

        await Assert.That(result.OutputValues[output.Id])
            .IsEqualTo(LogicValue.High);
        await Assert.That(restoredLibrary.Definitions.Count).IsEqualTo(1);
        await Assert.That(restoredResult.OutputValues[output.Id])
            .IsEqualTo(LogicValue.High);
    }

    [Test]
    public async Task Recursive_custom_chip_definitions_are_rejected()
    {
        var selfNode = LogicNode.CreateCustomChip("Self");
        var definition = new LogicChipDefinition(
            "Self",
            new LogicCircuit([selfNode]),
            new Dictionary<string, LogicNodeId>(),
            new Dictionary<string, LogicNodeId>());

        await Assert.That(() => new LogicChipLibrary([definition]))
            .Throws<InvalidOperationException>();
    }

    private static (LogicChipLibrary Library, LogicCircuit Circuit, LogicNode Output)
        BuildCustomAndCircuit()
    {
        var childA = LogicNode.CreateInput("A", value: LogicValue.Disconnected);
        var childB = LogicNode.CreateInput("B", value: LogicValue.Disconnected);
        var childGate = LogicNode.CreateGate(LogicNodeKind.And);
        var childOutput = LogicNode.CreateOutput("Y");
        var childCircuit = LogicCircuit.Empty
            .AddNode(childA)
            .AddNode(childB)
            .AddNode(childGate)
            .AddNode(childOutput)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(childA.Id, "out"),
                new LogicPortAddress(childGate.Id, "a"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(childB.Id, "out"),
                new LogicPortAddress(childGate.Id, "b"))
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(childGate.Id, "out"),
                new LogicPortAddress(childOutput.Id, "in"));
        var definition = new LogicChipDefinition(
            "AndChip",
            childCircuit,
            new Dictionary<string, LogicNodeId>
            {
                ["A"] = childA.Id,
                ["B"] = childB.Id,
            },
            new Dictionary<string, LogicNodeId>
            {
                ["Y"] = childOutput.Id,
            });
        var library = new LogicChipLibrary([definition]);

        var topA = LogicNode.CreateInput("A", value: LogicValue.High);
        var topB = LogicNode.CreateInput("B", value: LogicValue.High);
        var chip = LogicNode.CreateCustomChip("AndChip");
        var output = LogicNode.CreateOutput("Result");
        var circuit = LogicCircuit.Empty
            .AddNode(topA)
            .AddNode(topB)
            .AddNode(chip)
            .AddNode(output)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(topA.Id, "out"),
                new LogicPortAddress(chip.Id, "A"),
                library)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(topB.Id, "out"),
                new LogicPortAddress(chip.Id, "B"),
                library)
            .Connect(
                LogicConnectionId.New(),
                new LogicPortAddress(chip.Id, "Y"),
                new LogicPortAddress(output.Id, "in"),
                library);

        return (library, circuit, output);
    }
}
