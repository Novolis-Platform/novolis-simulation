namespace Novolis.Simulation.Mesh;

/// <summary>Canonical identity id prefixes: <c>person:</c>, <c>household:</c>, <c>firm:</c>, <c>ship:</c>, <c>thing:</c>.</summary>
public static class MeshIdentityIds
{
  /// <summary>string.</summary>
  public const string PersonPrefix = "person:";
  /// <summary>string.</summary>
  public const string HouseholdPrefix = "household:";
  /// <summary>string.</summary>
  public const string FirmPrefix = "firm:";
  /// <summary>string.</summary>
  public const string ShipPrefix = "ship:";
  /// <summary>string.</summary>
  public const string ThingPrefix = "thing:";

  /// <summary>Person.</summary>
  public static MeshIdentityId Person(string key) => MeshIdentityId.From(PersonPrefix + key);
  /// <summary>Household.</summary>
  public static MeshIdentityId Household(string key) => MeshIdentityId.From(HouseholdPrefix + key);
  /// <summary>Firm.</summary>
  public static MeshIdentityId Firm(string key) => MeshIdentityId.From(FirmPrefix + key);
  /// <summary>Ship.</summary>
  public static MeshIdentityId Ship(string key) => MeshIdentityId.From(ShipPrefix + key);
  /// <summary>Thing.</summary>
  public static MeshIdentityId Thing(string key) => MeshIdentityId.From(ThingPrefix + key);

  /// <summary>TryParseKind.</summary>
  public static MeshIdentityKind? TryParseKind(MeshIdentityId id)
  {
    var v = id.Value;
    if (v.StartsWith(PersonPrefix, StringComparison.Ordinal)) return MeshIdentityKind.Person;
    if (v.StartsWith(HouseholdPrefix, StringComparison.Ordinal)) return MeshIdentityKind.Household;
    if (v.StartsWith(FirmPrefix, StringComparison.Ordinal)) return MeshIdentityKind.Firm;
    if (v.StartsWith(ShipPrefix, StringComparison.Ordinal)) return MeshIdentityKind.Ship;
    if (v.StartsWith(ThingPrefix, StringComparison.Ordinal)) return MeshIdentityKind.Thing;
    return null;
  }
}
