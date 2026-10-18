namespace Novolis.Simulation.Logic;

internal static class LogicTruthTables
{
    public static LogicValue Evaluate(
        LogicNodeKind kind,
        IReadOnlyList<LogicValue> inputs)
    {
        return kind switch
        {
            LogicNodeKind.And => And(inputs),
            LogicNodeKind.Or => Or(inputs),
            LogicNodeKind.Xor => Xor(inputs),
            LogicNodeKind.Nand => Invert(And(inputs)),
            LogicNodeKind.Not => inputs.Count == 0
                ? LogicValue.Disconnected
                : Invert(inputs[0]),
            _ => LogicValue.Unknown,
        };
    }

    private static LogicValue And(IReadOnlyList<LogicValue> inputs)
    {
        if (inputs.Count == 0)
            return LogicValue.Disconnected;

        if (inputs.Any(value => value.Signal == LogicSignal.Contention))
            return LogicValue.Contention;

        if (inputs.Any(value => value.Signal == LogicSignal.Low))
            return LogicValue.Low;

        if (inputs.All(value => value.Signal == LogicSignal.High))
            return LogicValue.High;

        if (inputs.All(value => value.Signal == LogicSignal.Disconnected))
            return LogicValue.Disconnected;

        return LogicValue.Unknown;
    }

    private static LogicValue Or(IReadOnlyList<LogicValue> inputs)
    {
        if (inputs.Count == 0)
            return LogicValue.Disconnected;

        if (inputs.Any(value => value.Signal == LogicSignal.Contention))
            return LogicValue.Contention;

        if (inputs.Any(value => value.Signal == LogicSignal.High))
            return LogicValue.High;

        if (inputs.All(value => value.Signal == LogicSignal.Low))
            return LogicValue.Low;

        if (inputs.All(value => value.Signal == LogicSignal.Disconnected))
            return LogicValue.Disconnected;

        return LogicValue.Unknown;
    }

    private static LogicValue Xor(IReadOnlyList<LogicValue> inputs)
    {
        if (inputs.Count == 0)
            return LogicValue.Disconnected;

        if (inputs.Any(value => value.Signal == LogicSignal.Contention))
            return LogicValue.Contention;

        if (inputs.Any(value => !value.IsKnown))
            return inputs.All(value => value.IsDisconnected)
                ? LogicValue.Disconnected
                : LogicValue.Unknown;

        var highCount = inputs.Count(value => value.Signal == LogicSignal.High);
        return LogicValue.FromBoolean(highCount % 2 == 1);
    }

    private static LogicValue Invert(LogicValue value) =>
        value.Signal switch
        {
            LogicSignal.Low => LogicValue.High,
            LogicSignal.High => LogicValue.Low,
            LogicSignal.Disconnected => LogicValue.Disconnected,
            LogicSignal.Contention => LogicValue.Contention,
            _ => LogicValue.Unknown,
        };
}
