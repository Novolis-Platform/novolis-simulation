namespace Novolis.Simulation.Mesh;

/// <summary>One ordered transformation of <see cref="MeshState"/>.</summary>
public interface IMeshStep
{
  /// <summary>string Name { get; }.</summary>
  string Name { get; }
  /// <summary>MeshState Execute(MeshState current);.</summary>
  MeshState Execute(MeshState current);
}

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
