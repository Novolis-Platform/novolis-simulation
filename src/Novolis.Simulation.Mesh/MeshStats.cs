using System.Collections.Immutable;

namespace Novolis.Simulation.Mesh;

/// <summary>Counters for reports and tests.</summary>
public sealed record MeshStats(
  long DirectedPublishes = 0,
  long IdentityPublishes = 0,
  long FeedPublishes = 0,
  long DronesLaunched = 0,
  long DronesArrived = 0,
  long DronesLost = 0,
  long CacheCredits = 0,
  long MailboxPushes = 0,
  long FeedPulls = 0,
  long EmergencyForced = 0,
  long BandwidthDeferred = 0,
  long LocalCacheDrops = 0,
  long GlobalPacketDrops = 0,
  long RetractionsApplied = 0);
