using System.Numerics;
using System.Runtime.CompilerServices;

using Shape = FlipLib.Physics.CollisionShape;

namespace FlipLib.Physics;

public static partial class Overlap
{
    public static partial bool LineSegment(ref readonly OverlapQuery<LineSegment> query, ref Collision collision)
    {
        var segment = query.Entity;
        var self = query.Self;
        var world = query.World;

        Vector2? closestIntersectionPoint = null;
        float closestDistanceSqrd = float.MaxValue;

        for (int i = 0; i < world.Length; i++)
        {
            if (self == (Idx<Collider2D>)i) { continue; }
            ref readonly var other = ref world[i].Item;

            var intersectionPoint = other.ShapeType switch
            {
                Shape.Circle => Overlap.Circle(other.Circle, segment).Item1,
                Shape.AABB => Overlap.Rect(other.AABB, segment).Item1,
                _ => throw new NotImplementedException()
            };

            if (!intersectionPoint.HasValue) { continue; }

            var distanceSqrd = Vector2.DistanceSquared(segment.Start, intersectionPoint.Value);
            if (distanceSqrd < closestDistanceSqrd)
            {
                collision.OtherId = (Idx<Collider2D>)i;
                closestIntersectionPoint = intersectionPoint;
                closestDistanceSqrd = distanceSqrd;
            }
        }

        if (closestIntersectionPoint is null) { return false; }
        var nearest = world[collision.OtherId.Value].Item;

        switch (nearest.ShapeType)
        {
            case Shape.Circle:
                var circle = nearest.Circle;
                collision.Point = closestIntersectionPoint.Value;
                collision.Depth = Vector2.Distance(collision.Point, segment.End);
                collision.Normal = Vector2.Normalize(circle.Center - collision.Point);
                return true;
            case Shape.AABB:
                var rect = nearest.AABB;
                collision.Point = closestIntersectionPoint.Value;
                collision.Depth = Vector2.Distance(closestIntersectionPoint.Value, segment.End);
                collision.Normal = Vector2.Normalize(rect.Center - collision.Point).Round();
                return true;
            default: throw new NotImplementedException();
        }
    }

    public static partial (Vector2?, Vector2?) LineSegment(LineSegment segment, Circle circle)
    {
        var segmentDelta = segment.End - segment.Start;
        var segmentLengthSqrd = segmentDelta.LengthSquared();
        var t = Vector2.Dot(circle.Center - segment.Start, segmentDelta) / segmentLengthSqrd;

        var closestPointOnLine = segment.Start + (segmentDelta * t);

        var closestDistanceFromLineSqrd = (closestPointOnLine - circle.Center).LengthSquared();
        var radiiSqrd = circle.Radius * circle.Radius;

        if (radiiSqrd < closestDistanceFromLineSqrd) { return (null, null); }

        var intersectionDistance = MathF.Sqrt(radiiSqrd - closestDistanceFromLineSqrd);
        var segmentDir = Vector2.Normalize(segmentDelta);

        var p1 = closestPointOnLine - (intersectionDistance * segmentDir);
        var p2 = closestPointOnLine + (intersectionDistance * segmentDir);

        if (Vector2.DistanceSquared(p1, segment.Start) > Vector2.DistanceSquared(p2, segment.Start)) 
        {
            var tmp = p1;
            p1 = p2;
            p2 = tmp;
        }

        return (
            segment.IsInRange(p1) ? p1 : null,
            segment.IsInRange(p2) ? p2 : null
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial Vector2? LineSegment(LineSegment segment, Line line)
        => Line(line, segment);

    public static partial Vector2? LineSegment(LineSegment a, LineSegment b)
    {
        var p1 = a.Start;
        var p2 = a.End;
        var p3 = b.Start;
        var p4 = b.End;

        var denom = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);

        if (denom == 0) { return null; }

        var ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denom;
        var ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denom;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1) { return null; }

        var x = p1.X + ua * (p2.X - p1.X);
        var y = p1.Y + ua * (p2.Y - p1.Y);
        return new Vector2(x, y);
    }

    public static partial (Vector2?, Vector2?) LineSegment(LineSegment segment, Rectangle rect)
    {
        Span<Vector2> corners = stackalloc Vector2[4];
        rect.GetCorners(corners);

        Vector2? p1 = null;
        Vector2? p2 = null;

        for (int i = 0; i < 4; i++)
        {
            var ls = new LineSegment 
            {
                Start = corners[i],
                End = corners[(i + 1) % 4]
            };

            var intersection = LineSegment(ls, segment);
            if (!intersection.HasValue) { continue; }

            if (!p1.HasValue)
            {
                p1 = intersection.Value;
                continue;
            }

            p2 = intersection.Value;
            if (Vector2.DistanceSquared(p1.Value, segment.Start) > Vector2.DistanceSquared(p2.Value, segment.Start))
            {
                return (p2, p1);
            }
            return (p1, p2);
        }

        return (p1, null);
    }

        public static bool Rect(Rectangle rect, Vector2 p)
    {
        return p.X >= rect.Center.X - rect.HalfWidth
            && p.X <= rect.Center.X + rect.HalfWidth
            && p.Y >= rect.Center.Y - rect.HalfHeight
            && p.Y <= rect.Center.Y + rect.HalfHeight;
    }

    public static partial bool LineSegment(LineSegment s, Vector2 p)
    {
        if (!s.IsInRange(p)) { return false; }

        var lineDir = Vector2.Normalize(s.End - s.Start);
        var pointDir = Vector2.Normalize(p - (s.Start - lineDir * 2));

        return lineDir.Approximately(pointDir, 0.00001f);
    }
}
