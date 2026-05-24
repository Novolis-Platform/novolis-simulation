namespace Novolis.Simulation.Racing.Sensors;

using System.Numerics;

using Novolis.Simulation.Racing.Cars;
using Novolis.Simulation.Racing.Progress;
using Novolis.Simulation.Racing.Tracks;

public sealed class DefaultCarSensorModel : ICarSensorModel
{
    private const double MaxSpeed = 10.0;

    private static readonly SensorDefinition[] WallSensors =
    [
        new("WallLeft90",  -90, 15),
        new("WallLeft45",  -45, 20),
        new("WallLeft20",  -20, 25),
        new("WallAhead",     0, 30),
        new("WallRight20",  20, 25),
        new("WallRight45",  45, 20),
        new("WallRight90",  90, 15),
    ];

    private static readonly SensorDefinition[] AllDefinitions =
    [
        new("WallLeft90",  -90, 15),
        new("WallLeft45",  -45, 20),
        new("WallLeft20",  -20, 25),
        new("WallAhead",     0, 30),
        new("WallRight20",  20, 25),
        new("WallRight45",  45, 20),
        new("WallRight90",  90, 15),
        new("Speed", 0, 0),
        new("Alignment", 0, 0),
        new("CenterOffset", 0, 0),
    ];

    public IReadOnlyList<SensorDefinition> Definitions => AllDefinitions;

    public SensorReading Read(RaceTrack track, CarState car)
    {
        var values = new double[10];
        var hits = new SensorRayHit[10];

        for (int i = 0; i < 7; i++)
        {
            var def = WallSensors[i];
            double angleRad = def.RelativeAngleDegrees * Math.PI / 180.0;
            var dir = Rotate(car.Forward, angleRad);
            var (distance, hitWall) = MarchRay(track, car.Position, dir, def.MaxRange);
            values[i] = 1.0 - Math.Clamp(distance / def.MaxRange, 0.0, 1.0);
            hits[i] = new SensorRayHit(def.Name, car.Position, dir, distance, hitWall);
        }

        values[7] = Math.Clamp(car.Speed / MaxSpeed, 0.0, 1.0);
        hits[7] = new SensorRayHit("Speed", car.Position, Vector2.Zero, car.Speed, false);

        var resolver = new TrackProgressResolver();
        var progress = resolver.Resolve(track, car.Position, car.Forward);

        values[8] = progress.Alignment;
        hits[8] = new SensorRayHit("Alignment", car.Position, Vector2.Zero, progress.Alignment, false);

        double halfWidth = track.Geometry.HalfWidth;
        double offsetNorm = (Math.Clamp(progress.SignedCenterOffset / halfWidth, -1.0, 1.0) + 1.0) / 2.0;
        values[9] = offsetNorm;
        hits[9] = new SensorRayHit("CenterOffset", car.Position, Vector2.Zero, progress.SignedCenterOffset, false);

        return new SensorReading(values, hits);
    }

    private static Vector2 Rotate(Vector2 v, double angleRad)
    {
        float cos = (float)Math.Cos(angleRad);
        float sin = (float)Math.Sin(angleRad);
        return new Vector2(cos * v.X - sin * v.Y, sin * v.X + cos * v.Y);
    }

    private static (double distance, bool hitWall) MarchRay(RaceTrack track, Vector2 origin, Vector2 direction, double maxRange)
    {
        float stepSize = 0.5f;
        int maxSteps = (int)(maxRange / stepSize) + 1;
        var dir = Vector2.Normalize(direction) * stepSize;

        for (int step = 1; step <= maxSteps; step++)
        {
            var pos = origin + dir * step;
            int col = (int)pos.X;
            int row = (int)pos.Y;

            if (col < 0 || col >= track.Width || row < 0 || row >= track.Height)
                return (step * stepSize, false);

            var cell = track.Cells[col, row];
            if (cell == TrackCell.Wall)
                return (step * stepSize, true);
        }

        return (maxRange, false);
    }
}
