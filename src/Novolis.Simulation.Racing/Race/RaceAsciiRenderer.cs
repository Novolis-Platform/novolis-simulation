using System.Globalization;
using System.Text;

using Novolis.Simulation.Racing.Tracks;

namespace Novolis.Simulation.Racing.Race;

/// <summary>ASCII overview of a race grid — presentation-neutral output for consoles or logs.</summary>
public static class RaceAsciiRenderer
{
    /// <summary>Render operation.</summary>
    public static string Render(RaceSimulation sim)
    {
        var track = sim.Track;
        var w = track.Width;
        var h = track.Height;
        var grid = new char[w, h];
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
                grid[x, y] = CellChar(track.Cells[x, y]);
        }

        for (var i = 0; i < sim.State.Cars.Count; i++)
        {
            var car = sim.State.Cars[i];
            var cx = (int)car.Position.X;
            var cy = (int)car.Position.Y;
            if (cx < 0 || cx >= w || cy < 0 || cy >= h)
                continue;
            var mark = (char)('1' + Math.Min(i, 8));
            grid[cx, cy] = car.Crashed ? 'x' : mark;
        }

        var sb = new StringBuilder((w + 1) * h + 128);
        sb.Append(CultureInfo.InvariantCulture, $"Tick={sim.State.Tick}  ");
        for (var i = 0; i < sim.State.Cars.Count; i++)
        {
            if (i > 0)
                sb.Append(" | ");
            var c = sim.State.Cars[i];
            sb.Append(CultureInfo.InvariantCulture, $"{sim.Controllers[i].Name}: lap {c.CompletedLaps} prog {c.TrackProgress:0.00} fit {c.Fitness:0.0}");
            if (c.Crashed)
                sb.Append(" CRASH");
        }

        sb.AppendLine();
        for (var y = 0; y < h; y++)
        {
            for (var x = 0; x < w; x++)
                sb.Append(grid[x, y]);
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static char CellChar(TrackCell cell) =>
        cell switch
        {
            TrackCell.Wall => '#',
            TrackCell.Road => '.',
            TrackCell.StartFinish => '=',
            TrackCell.Gate => '|',
            _ => ' '
        };
}
