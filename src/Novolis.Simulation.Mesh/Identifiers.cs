namespace Novolis.Simulation.Mesh;

/// <summary>Stable mesh node key (usually Astro system id).</summary>
public readonly record struct MeshNodeId(string Value)
{
  /// <summary>ToString.</summary>
  public override string ToString() => Value;
  /// <summary>From.</summary>
  public static MeshNodeId From(string value) => new(value);
}

/// <summary>Mailbox / feed owner key. Prefer <see cref="MeshIdentityIds"/> factories for naming.</summary>
public readonly record struct MeshIdentityId(string Value)
{
  /// <summary>ToString.</summary>
  public override string ToString() => Value;
  /// <summary>From.</summary>
  public static MeshIdentityId From(string value) => new(value);
}

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

/// <summary>Named channel (Atom/RSS-style). <see cref="Emergency"/> is mandatory for every mailbox.</summary>
public readonly record struct MeshFeedId(string Value)
{
  /// <summary>ToString.</summary>
  public override string ToString() => Value;
  /// <summary>From.</summary>
  public static MeshFeedId From(string value) => new(value);

  /// <summary>Forced civil alert channel — cannot unsubscribe; force-delivered at co-located nodes.</summary>
  public static MeshFeedId Emergency { get; } = From("Emergency");

  /// <summary>NewsGeneral.</summary>
  public static MeshFeedId NewsGeneral { get; } = From("News.General");
  /// <summary>NewsSpaceWhales.</summary>
  public static MeshFeedId NewsSpaceWhales { get; } = From("News.SpaceWhales");
  /// <summary>NewsPrices.</summary>
  public static MeshFeedId NewsPrices { get; } = From("News.Prices");
  /// <summary>Delayed spot commodity digests — mesh board reads these after mesh lag.</summary>
  public static MeshFeedId CommerceSpot { get; } = From("Commerce.Spot");

  /// <summary>IsMandatory.</summary>
  public bool IsMandatory => Value.Equals(Emergency.Value, StringComparison.Ordinal);

  /// <summary>IsMandatoryFeed.</summary>
  public static bool IsMandatoryFeed(MeshFeedId feed) => feed.IsMandatory;
}

/// <summary>Published packet id.</summary>
public readonly record struct PacketId(Guid Value)
{
  /// <summary>New.</summary>
  public static PacketId New() => new(Guid.NewGuid());
  /// <summary>From.</summary>
  public static PacketId From(Guid value) => new(value);
}

/// <summary>In-flight disposable drone instance.</summary>
public readonly record struct DroneId(Guid Value)
{
  /// <summary>New.</summary>
  public static DroneId New() => new(Guid.NewGuid());
}
