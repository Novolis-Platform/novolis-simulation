using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Immutable aggregate mesh state (stocks).</summary>
public sealed record MeshState(
  long HourIndex,
  MeshPolicy Policy,
  ImmutableDictionary<string, MeshNode> Nodes,
  ImmutableArray<MeshEdge> Edges,
  ImmutableDictionary<string, MeshPacket> Packets,
  ImmutableArray<InFlightDrone> Drones,
  ImmutableArray<PendingLaunch> Pending,
  ImmutableDictionary<string, ImmutableDictionary<string, NodeCacheEntry>> NodeCaches,
  ImmutableDictionary<string, MeshMailbox> Mailboxes,
  ImmutableDictionary<string, MeshSubscriptionBook> Subscriptions,
  ImmutableDictionary<string, ImmutableHashSet<string>> FeedInboxes,
  ImmutableDictionary<string, int> BandwidthUsedThisHour,
  ImmutableDictionary<string, ImmutableHashSet<string>> FloodSeededAt,
  ImmutableDictionary<string, int> PacketLossCounts,
  ImmutableDictionary<string, ImmutableDictionary<string, NodeCacheEntry>> NodeRetractions,
  MeshStats Stats)
{
  /// <summary>Empty.</summary>
  public static MeshState Empty(MeshPolicy? policy = null) => new(
    HourIndex: 0,
    Policy: policy ?? new MeshPolicy(),
    Nodes: ImmutableDictionary<string, MeshNode>.Empty,
    Edges: ImmutableArray<MeshEdge>.Empty,
    Packets: ImmutableDictionary<string, MeshPacket>.Empty,
    Drones: ImmutableArray<InFlightDrone>.Empty,
    Pending: ImmutableArray<PendingLaunch>.Empty,
    NodeCaches: ImmutableDictionary<string, ImmutableDictionary<string, NodeCacheEntry>>.Empty,
    Mailboxes: ImmutableDictionary<string, MeshMailbox>.Empty,
    Subscriptions: ImmutableDictionary<string, MeshSubscriptionBook>.Empty,
    FeedInboxes: ImmutableDictionary<string, ImmutableHashSet<string>>.Empty,
    BandwidthUsedThisHour: ImmutableDictionary<string, int>.Empty,
    FloodSeededAt: ImmutableDictionary<string, ImmutableHashSet<string>>.Empty,
    PacketLossCounts: ImmutableDictionary<string, int>.Empty,
    NodeRetractions: ImmutableDictionary<string, ImmutableDictionary<string, NodeCacheEntry>>.Empty,
    Stats: new MeshStats());

  /// <summary>IsVisibleAt.</summary>
  public bool IsVisibleAt(PacketId packet, MeshNodeId node) =>
    NodeCaches.TryGetValue(node.Value, out var set) && set.ContainsKey(PacketKey(packet));

  /// <summary>True when a retraction for <paramref name="logicalKey"/> is visible at the node.</summary>
  public bool IsRetractedAt(string logicalKey, MeshNodeId node) =>
    !string.IsNullOrEmpty(logicalKey)
    && NodeRetractions.TryGetValue(node.Value, out var map)
    && map.ContainsKey(logicalKey);

  /// <summary>TryGetCacheEntry.</summary>
  public bool TryGetCacheEntry(PacketId packet, MeshNodeId node, out NodeCacheEntry entry)
  {
    if (NodeCaches.TryGetValue(node.Value, out var set)
        && set.TryGetValue(PacketKey(packet), out entry!))
    {
      return true;
    }

    entry = null!;
    return false;
  }

  /// <summary>IsInMailbox.</summary>
  public bool IsInMailbox(PacketId packet, MeshIdentityId identity) =>
    Mailboxes.TryGetValue(identity.Value, out var box)
    && box.PushedPacketKeys.Contains(PacketKey(packet));

  /// <summary>IsInFeedInbox.</summary>
  public bool IsInFeedInbox(PacketId packet, MeshIdentityId identity) =>
    FeedInboxes.TryGetValue(identity.Value, out var set) && set.Contains(PacketKey(packet));

  /// <summary>TryGetPacket.</summary>
  public bool TryGetPacket(PacketId id, out MeshPacket packet)
  {
    if (Packets.TryGetValue(PacketKey(id), out packet!))
    {
      return true;
    }

    packet = null!;
    return false;
  }

  /// <summary>TryGetMailbox.</summary>
  public bool TryGetMailbox(MeshIdentityId id, out MeshMailbox mailbox)
  {
    if (Mailboxes.TryGetValue(id.Value, out mailbox!))
    {
      return true;
    }

    mailbox = null!;
    return false;
  }

  internal static string PacketKey(PacketId id) => id.Value.ToString("N");
}
