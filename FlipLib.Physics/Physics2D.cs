using System.Numerics;

namespace FES.Physics;

using Shape = CollisionShape;

public struct Collision
{
    public Idx<Collider2D> OtherId;
    public Vector2 Point;
    public Vector2 Normal;
    public float Depth;
}

public static class Physics2D
{
    public static bool GetNearestOverlapping(
        ReadOnlySpan<Entity<Collider2D>> world, 
        ref readonly Collider2D collider, 
        out Idx<Collider2D> otherId, 
        Idx<Collider2D>? ignoreId = null
    )
    {   
        float? nearestDist = null;
        float radiiSqrd; 
        float distanceSqrd;

        otherId = default;

        for (int i = 0; i < world.Length; i++)
        {
            if (ignoreId.HasValue && ignoreId.Value == (Idx<Collider2D>)i)  { continue; }

            ref readonly var otherEntity = ref world[i];
            ref readonly var other = ref otherEntity.Item;

            if ((other.ShapeType, collider.ShapeType) is (Shape.Circle, Shape.Circle))
            {
                radiiSqrd = other.Circle.Radius + collider.Circle.Radius; 
                radiiSqrd *= radiiSqrd;
                distanceSqrd = Vector2.DistanceSquared(other.Circle.Center, collider.Circle.Center);

                if (distanceSqrd <= radiiSqrd && (!nearestDist.HasValue || nearestDist.Value < distanceSqrd))
                {
                    nearestDist = distanceSqrd;
                    otherId = new Idx<Collider2D>(i);
                }

                continue;
            }

            throw new NotImplementedException();
        }

        return nearestDist.HasValue;
    }

    public static Vector2? FindIntersection(LineSegment a, LineSegment b)
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

    public static Vector2? FindIntersection(Line l, LineSegment s)
    {
        var p1 = l.Point;
        var p2 = l.Point + l.Direction;
        var p3 = s.Start;
        var p4 = s.End;

        var denom = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);

        if (denom == 0) { return null; }

        var t = ((p1.X - p3.X) * (p3.Y - p4.Y) - (p1.Y - p3.Y) * (p3.X - p4.X)) / denom;
        var u = -((p1.X - p2.X) * (p1.Y - p3.Y) - (p1.Y - p2.Y) * (p1.X - p3.X)) / denom;

        if (u < 0 || u > 1) { return null; }

        var x = p1.X + t * (p2.X - p1.X);
        var y = p1.Y + t * (p2.Y - p1.Y);
        return new Vector2(x, y);
    }

    public static bool Overlaps(Rectangle rect, Vector2 p)
    {
        var halfWidth = rect.Width / 2;
        var halfHeight = rect.Height / 2;

        return p.X >= rect.Center.X - halfWidth
            && p.X <= rect.Center.X + halfWidth
            && p.Y >= rect.Center.Y - halfHeight
            && p.Y <= rect.Center.Y + halfHeight;
    }

    public static bool Overlaps(LineSegment s, Vector2 p)
    {
        if (!InSegmentRange(s, p)) { return false; }

        var lineDir = Vector2.Normalize(s.End - s.Start);
        var pointDir = Vector2.Normalize(p - s.Start);

        return pointDir == lineDir || pointDir == -lineDir;
    }

    public static bool InSegmentRange(LineSegment s, Vector2 p)
        => p.X >= Math.Min(s.Start.X, s.End.X) 
        && p.X <= Math.Max(s.Start.X, s.End.X)
        && p.Y >= Math.Min(s.Start.Y, s.End.Y) 
        && p.Y <= Math.Max(s.Start.Y, s.End.Y);
}