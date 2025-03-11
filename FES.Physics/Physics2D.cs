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
        Vector2 p1 = a.Start;
        Vector2 p2 = a.End;
        Vector2 p3 = b.Start;
        Vector2 p4 = b.End;

        float denom = (p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y);

        if (denom == 0) { return null; }

        float ua = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X)) / denom;
        float ub = ((p2.X - p1.X) * (p1.Y - p3.Y) - (p2.Y - p1.Y) * (p1.X - p3.X)) / denom;

        if (ua < 0 || ua > 1 || ub < 0 || ub > 1) { return null; }

        float x = p1.X + ua * (p2.X - p1.X);
        float y = p1.Y + ua * (p2.Y - p1.Y);
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