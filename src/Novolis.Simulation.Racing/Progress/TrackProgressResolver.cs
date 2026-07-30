namespace Novolis.Simulation.Racing.Progress;

using System.Numerics;

using Novolis.Simulation.Racing.Tracks;

/// <summary>Represents TrackProgressResolver.</summary>
public sealed class TrackProgressResolver : ITrackProgressResolver
{
    /// <summary>Resolve operation.</summary>
    public TrackProgressSample Resolve(RaceTrack track, Vector3 position, Vector3 forward)
    {
        var map = track.ProgressMap;
        var samplePositions = map.Samples;
        int count = samplePositions.Count;

        int nearestIdx = 0;
        float nearestDistSq = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            float dSq = Vector3.DistanceSquared(position, samplePositions[i]);
            if (dSq < nearestDistSq)
            {
                nearestDistSq = dSq;
                nearestIdx = i;
            }
        }

        double loopT = map.CumulativeArcLengths[nearestIdx] / map.TotalArcLength;
        var tangent = map.Tangents[nearestIdx];
        var normal = new Vector3(tangent.Z, 0f, -tangent.X);

        double offset = Vector3.Dot(position - samplePositions[nearestIdx], normal);
        double alignmentRaw = Vector3.Dot(forward, tangent);
        double alignment = (alignmentRaw + 1.0) / 2.0;
        bool isWrongWay = alignmentRaw < -0.3;

        return new TrackProgressSample(loopT, loopT, alignment, offset, isWrongWay);
    }
}
