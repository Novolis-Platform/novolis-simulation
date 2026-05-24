namespace Novolis.Simulation.Racing.Progress;

using System.Numerics;

using Novolis.Simulation.Racing.Tracks;

public sealed class TrackProgressResolver : ITrackProgressResolver
{
    public TrackProgressSample Resolve(RaceTrack track, Vector2 position, Vector2 forward)
    {
        var map = track.ProgressMap;
        var samplePositions = map.Samples;
        int count = samplePositions.Count;

        int nearestIdx = 0;
        float nearestDistSq = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            float dSq = Vector2.DistanceSquared(position, samplePositions[i]);
            if (dSq < nearestDistSq)
            {
                nearestDistSq = dSq;
                nearestIdx = i;
            }
        }

        double loopT = map.CumulativeArcLengths[nearestIdx] / map.TotalArcLength;
        var tangent = map.Tangents[nearestIdx];
        var normal = new Vector2(tangent.Y, -tangent.X);

        double offset = Vector2.Dot(position - samplePositions[nearestIdx], normal);
        double alignmentRaw = Vector2.Dot(forward, tangent);
        double alignment = (alignmentRaw + 1.0) / 2.0;
        bool isWrongWay = alignmentRaw < -0.3;

        return new TrackProgressSample(loopT, loopT, alignment, offset, isWrongWay);
    }
}
