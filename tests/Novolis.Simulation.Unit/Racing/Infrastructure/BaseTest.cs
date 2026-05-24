namespace Novolis.Simulation.Racing.Tests.Infrastructure;

public abstract class BaseTest
{
    protected void Output<T>(T value) => StructuredTestOutput.WriteObject(value);

    protected void Output(string value) => StructuredTestOutput.WriteLine(value);
}
