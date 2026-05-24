using System.Text.Json;
using System.Text.Json.Serialization;

using TUnit.Core;

namespace Novolis.Simulation.Racing.Tests.Infrastructure;

public static class StructuredTestOutput
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
    };

    public static void WriteObject<T>(T value)
    {
        var line = value is null ? "null" : JsonSerializer.Serialize(value, SerializerOptions);
        TestContext.Current?.OutputWriter.WriteLine(line);
    }

    public static void WriteLine(string value) =>
        TestContext.Current?.OutputWriter.WriteLine(value);
}
