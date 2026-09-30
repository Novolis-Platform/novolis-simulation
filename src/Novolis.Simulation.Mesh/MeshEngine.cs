namespace Novolis.Simulation.Mesh;

/// <summary>Advances mesh by folding an ordered step list.</summary>
public sealed class MeshEngine(IReadOnlyList<IMeshStep> steps)
{
  /// <summary>Steps.</summary>
  public IReadOnlyList<IMeshStep> Steps { get; } = steps ?? throw new ArgumentNullException(nameof(steps));

  /// <summary>Advance.</summary>
  public MeshState Advance(MeshState state)
  {
    ArgumentNullException.ThrowIfNull(state);
    return Steps.Aggregate(state, static (current, step) => step.Execute(current));
  }
}
