namespace Novolis.Simulation.Mesh;

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
