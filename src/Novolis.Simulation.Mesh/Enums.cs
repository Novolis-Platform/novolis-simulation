namespace Novolis.Simulation.Mesh;

/// <summary>Traffic layer — pulse is sprint drones; bulk is freight-class; feed is public channel cargo.</summary>
public enum MeshTrafficLayer
{
  /// <summary>Pulse.</summary>
  Pulse = 0,
  /// <summary>Bulk.</summary>
  Bulk = 1,
  /// <summary>Feed.</summary>
  Feed = 2,
}

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

/// <summary>Who owns a mailbox / feed subscriptions.</summary>
public enum MeshIdentityKind
{
  /// <summary>Person.</summary>
  Person = 0,
  /// <summary>Household.</summary>
  Household = 1,
  /// <summary>Firm.</summary>
  Firm = 2,
  /// <summary>Ship.</summary>
  Ship = 3,
  /// <summary>Facility, buoy, kiosk, drone rack — non-person endpoints.</summary>
  Thing = 4,
}
