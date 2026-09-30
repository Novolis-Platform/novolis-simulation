using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Destination of a publish.</summary>
public sealed record MeshAddress(
  MeshAddressKind Kind,
  MeshNodeId? Place = null,
  MeshIdentityId? Identity = null,
  MeshFeedId? Feed = null)
{
  /// <summary>ToPlace.</summary>
  public static MeshAddress ToPlace(MeshNodeId node) => new(MeshAddressKind.Place, Place: node);

  /// <summary>ToIdentity.</summary>
  public static MeshAddress ToIdentity(MeshIdentityId id) =>
    new(MeshAddressKind.Identity, Identity: id);

  /// <summary>ToFeed.</summary>
  public static MeshAddress ToFeed(MeshFeedId feed) => new(MeshAddressKind.Feed, Feed: feed);
}
