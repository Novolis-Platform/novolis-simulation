namespace Novolis.Simulation.Mesh;

/// <summary>How a packet is addressed.</summary>
public enum MeshAddressKind
{
  /// <summary>Known node / system — directed path.</summary>
  Place = 0,
  /// <summary>Identity — flood; push into mailbox only when co-located with a node that holds it.</summary>
  Identity = 1,
  /// <summary>Named feed — flood to node caches; consumers pull by subscription (not pushed to mailbox).</summary>
  Feed = 2,
}
