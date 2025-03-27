using System.Numerics;

namespace FlipLib.Physics;

public static partial class Overlap
{
    public static partial (Vector2?, Vector2?) Line(Line line, Rectangle rect) 
    {
        Span<Vector2> corners = stackalloc Vector2[4];
        rect.GetCorners(corners);

        (Vector2?, Vector2?) result = (null, null);
        for (int i = 0; i < 4; i++)
        {
            var intersection = Line(line, new LineSegment
            {
                Start = corners[i],
                End = corners[(i + 1) % 4]
            });

            if (intersection is null) { continue; }
            if (result.Item1.HasValue)
            {
                result.Item2 = intersection;
                return result;
            }
            result.Item1 = intersection;
        }

        return result;
    }

    public static partial Vector2? Line(Line line, LineSegment segment)
    {
        var p1 = line.Point;
        var p2 = line.Point + line.Direction;
        var p3 = segment.Start;
        var p4 = segment.End;

        var denom = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);

        if (denom == 0) { return null; }

        var t = ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X)) / denom;
        var u = -((p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X)) / denom;

        if (u < 0 || u > 1) { return null; }

        var x = p1.X + t * (p2.X - p1.X);
        var y = p1.Y + t * (p2.Y - p1.Y);
        return new Vector2(x, y);
    }
}
