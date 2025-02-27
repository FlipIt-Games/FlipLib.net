using System.Numerics;

namespace FES.Physics;

using Shape = CollisionShape;

public ref struct Collision
{
    public Id<Collider2D> OtherId;
    public Vector2 Point;
    public Vector2 Normal;
    public float Depth;
}

public static class Physics2D
{
    public static bool GetNearestOverlapping(ReadOnlySpan<Collider2D> world, ref readonly Collider2D collider, out Id<Collider2D> otherId)
    {   
        float? nearestDist = null;
        float radiiSqrd; 
        float distanceSqrd;

        otherId = default;

        for (int i = 0; i < world.Length; i++)
        {
            var other = world[i];
            if ((other.ShapeType, collider.ShapeType) is (Shape.Circle, Shape.Circle))
            {
                radiiSqrd = other.Circle.Radius + collider.Circle.Radius; 
                radiiSqrd *= radiiSqrd;
                distanceSqrd = Vector2.DistanceSquared(other.Circle.Center, collider.Circle.Center);

                if (distanceSqrd <= radiiSqrd && (!nearestDist.HasValue || nearestDist.Value < distanceSqrd))
                {
                    nearestDist = distanceSqrd;
                    otherId = new Id<Collider2D>(i);
                }
            }

            throw new NotImplementedException();
        }

        return nearestDist.HasValue;
    }
   
    public static bool GetNearestOverlapping(ReadOnlySpan<Collider2D> world, ref readonly Collider2D collider, ref Collision collision)
    {
        Collider2D? nearest = null;
        float? nearestDist = null;

        float radii = 0;
        float radiiSqrd = 0; 
        float distanceSqrd = 0;

        for (int i = 0; i < world.Length; i++)
        {
            var other = world[i];
            if ((other.ShapeType, collider.ShapeType) is (Shape.Circle, Shape.Circle))
            {
                radii = other.Circle.Radius + collider.Circle.Radius; 
                radiiSqrd = radii * radii;
                distanceSqrd = Vector2.DistanceSquared(other.Circle.Center, collider.Circle.Center);

                if (distanceSqrd <= radiiSqrd && (!nearestDist.HasValue || nearestDist.Value < distanceSqrd))
                {
                    nearestDist = distanceSqrd;
                    nearest = other;
                    collision.OtherId = new Id<Collider2D>(i);
                }
            }
        }

        if (!nearest.HasValue) 
        {
            return false;
        }

        if ((nearest.Value.ShapeType, collider.ShapeType) is (Shape.Circle, Shape.Circle))
        {
            var distance = MathF.Sqrt(distanceSqrd);

            collision.Depth = distance - radii;
            collision.Normal = Vector2.Normalize(collider.Circle.Center - nearest.Value.Circle.Center);
            collision.Point = nearest.Value.Circle.Center + (collision.Normal * nearest.Value.Circle.Radius);

            return true;
        }

        throw new NotImplementedException(); 
    }

    public static Vector2? FindIntersection(LineSegment a, LineSegment b)
    {
        var dYA = a.End.Y - a.Start.Y;
        var dXA = a.Start.X - a.End.X;
        var cA = dYA * a.Start.X + dXA * a.Start.Y;

        var dYB = b.End.Y - b.Start.Y;
        var dXB = b.Start.X - b.End.X;
        var cB = dYA * b.Start.X + dXA * b.Start.Y;

        var det = dYA * dXB - dYB * dXA;

        if (det == 0) { return null; }

        var intersection = new Vector2(
            (dXB * cA - dXA * cB) / det,
            (dYA * cB - dXA * cA) / det
        );
        
        if (InSegmentRange(a, intersection) && InSegmentRange(b, intersection))
        {
            return intersection;
        }

        return null;
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
