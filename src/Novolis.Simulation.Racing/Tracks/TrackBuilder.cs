namespace Novolis.Simulation.Racing.Tracks;

using System.Numerics;

public sealed class TrackBuilder : ITrackBuilder
{
    private const int SampleCount = 1000;

    public RaceTrack Build(ITrackDefinition definition)
    {
        var spec = definition.BuildSpec;
        var sampler = new CatmullRomSplineSampler();
        var samples = sampler.SampleEvenly(spec.CenterLine, SampleCount);

        var positions = samples.Select(s => s.Position).ToArray();
        var tangents = samples.Select(s => s.Tangent).ToArray();
        var arcLens = new double[SampleCount];
        arcLens[0] = 0;
        for (int i = 1; i < SampleCount; i++)
            arcLens[i] = arcLens[i - 1] + Vector2.Distance(positions[i], positions[i - 1]);
        double totalArcLength = arcLens[SampleCount - 1] + Vector2.Distance(positions[SampleCount - 1], positions[0]);

        var progressMap = new TrackProgressMap
        {
            Samples = positions,
            Tangents = tangents,
            CumulativeArcLengths = arcLens,
            TotalArcLength = totalArcLength
        };

        int width = spec.RasterWidth;
        int height = spec.RasterHeight;
        double halfWidth = spec.TrackHalfWidth;
        double wallThickness = spec.WallThickness;

        var cells = new TrackCell[width, height];

        for (int col = 0; col < width; col++)
        {
            for (int row = 0; row < height; row++)
            {
                var worldPos = new Vector2(col + 0.5f, row + 0.5f);

                int nearestIdx = 0;
                float nearestDistSq = float.MaxValue;
                for (int s = 0; s < SampleCount; s++)
                {
                    float dSq = Vector2.DistanceSquared(worldPos, positions[s]);
                    if (dSq < nearestDistSq)
                    {
                        nearestDistSq = dSq;
                        nearestIdx = s;
                    }
                }

                int nextIdx = (nearestIdx + 1) % SampleCount;
                double dist = DistanceToSegment(worldPos, positions[nearestIdx], positions[nextIdx]);
                int prevIdx = (nearestIdx - 1 + SampleCount) % SampleCount;
                double dist2 = DistanceToSegment(worldPos, positions[prevIdx], positions[nearestIdx]);
                if (dist2 < dist) dist = dist2;

                if (dist < halfWidth)
                    cells[col, row] = TrackCell.Road;
                else if (dist < halfWidth + wallThickness)
                    cells[col, row] = TrackCell.Wall;
                else
                    cells[col, row] = TrackCell.Empty;
            }
        }

        foreach (var t in spec.GateSamples)
        {
            int sampleIdx = (int)(t * SampleCount) % SampleCount;
            var sample = samples[sampleIdx];
            var gateA = sample.Position - sample.Normal * (float)halfWidth;
            var gateB = sample.Position + sample.Normal * (float)halfWidth;
            MarkGateLine(cells, width, height, gateA, gateB);
        }

        int startIdx = (int)(spec.StartSample * SampleCount) % SampleCount;
        var startSample = samples[startIdx];
        int startCol = (int)startSample.Position.X;
        int startRow = (int)startSample.Position.Y;
        if (startCol >= 0 && startCol < width && startRow >= 0 && startRow < height)
            cells[startCol, startRow] = TrackCell.StartFinish;

        var startPose = new TrackStartPose(startSample.Position, startSample.Tangent);

        var gates = new List<TrackGate>();
        foreach (var (index, t) in spec.GateSamples.Select((t, i) => (i, t)))
        {
            int sampleIdx = (int)(t * SampleCount) % SampleCount;
            var sample = samples[sampleIdx];
            var a = sample.Position - sample.Normal * (float)halfWidth;
            var b = sample.Position + sample.Normal * (float)halfWidth;
            gates.Add(new TrackGate(index, a, b, t));
        }

        var leftBoundary = samples.Select(s => s.Position - s.Normal * (float)halfWidth).ToArray();
        var rightBoundary = samples.Select(s => s.Position + s.Normal * (float)halfWidth).ToArray();
        var geometry = new TrackGeometry
        {
            LeftBoundary = leftBoundary,
            RightBoundary = rightBoundary,
            HalfWidth = halfWidth
        };

        return new RaceTrack
        {
            Id = definition.Id,
            Name = definition.Name,
            Width = width,
            Height = height,
            Cells = cells,
            CenterLineSamples = positions,
            Gates = gates,
            StartPose = startPose,
            ProgressMap = progressMap,
            Geometry = geometry
        };
    }

    private static void MarkGateLine(TrackCell[,] cells, int width, int height, Vector2 a, Vector2 b)
    {
        var dir = b - a;
        float len = dir.Length();
        if (len < 0.001f) return;
        var step = dir / len * 0.5f;
        int steps = (int)(len / 0.5f) + 2;
        for (int i = 0; i <= steps; i++)
        {
            var pt = a + step * i;
            int col = (int)pt.X;
            int row = (int)pt.Y;
            if (col >= 0 && col < width && row >= 0 && row < height)
            {
                if (cells[col, row] == TrackCell.Road)
                    cells[col, row] = TrackCell.Gate;
            }
        }
    }

    private static double DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
    {
        var ab = b - a;
        float lenSq = ab.LengthSquared();
        if (lenSq < 1e-10f)
            return Vector2.Distance(point, a);
        float t = Math.Clamp(Vector2.Dot(point - a, ab) / lenSq, 0f, 1f);
        var proj = a + ab * t;
        return Vector2.Distance(point, proj);
    }
}
