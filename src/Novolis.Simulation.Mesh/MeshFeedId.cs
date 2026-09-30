namespace Novolis.Simulation.Mesh;

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
