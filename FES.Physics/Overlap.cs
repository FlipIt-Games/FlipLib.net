using FES;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FES.Physics;

using Shape = CollisionShape;

public static partial class Overlap
{
    // only returns if there is an overlap or not
    public static bool Circle(ReadOnlySpan<Entity<Collider2D>> world, Circle circle, Idx<Collider2D>? self = null)
    {
        throw new NotImplementedException();
    }

    /// Returns the nearest in the provided collision object
    public static bool Circle(ReadOnlySpan<Entity<Collider2D>> world, Circle circle, ref Collision collision, Idx<Collider2D>? self = null)
    {
        Collider2D? nearest = null;
        float? nearestDistSqrd = null;
        float radii = 0;

        Vector2 closestPointOnRect = Vector2.Zero;

        for (int i = 0; i < world.Length; i++)
        {
            if (self == (Idx<Collider2D>)i)  { continue; }

            float distanceSqrd = 0;
            ref readonly var other = ref world[i].Item;
            if (other.ShapeType is Shape.Circle)
            {
                radii = other.Circle.Radius + circle.Radius; 
                var radiiSqrd = radii * radii;
                distanceSqrd = Vector2.DistanceSquared(circle.Center, other.Circle.Center);

                if (distanceSqrd > radiiSqrd || distanceSqrd > nearestDistSqrd) { continue; }

                nearestDistSqrd = distanceSqrd;
                nearest = other;
                collision.OtherId = new Idx<Collider2D>(i);

                continue;
            }
            else if (other.ShapeType is Shape.Rectangle)
            {
                ref readonly var rect = ref other.Rectangle;

                var halfWidth = other.Rectangle.Width / 2;                                
                var halfHeight = other.Rectangle.Height / 2;                                

                var closestPointOnRectTmp =  new Vector2(
                    Math.Clamp(circle.Center.X, rect.Center.X - halfWidth, rect.Center.X + halfWidth),
                    Math.Clamp(circle.Center.Y, rect.Center.Y - halfHeight, rect.Center.Y + halfHeight)
                );

                distanceSqrd = Vector2.DistanceSquared(circle.Center, closestPointOnRectTmp);

                if (distanceSqrd > circle.Radius * circle.Radius) { continue; }

                collision.OtherId = new Idx<Collider2D>(i);

                closestPointOnRect = closestPointOnRectTmp;
                nearestDistSqrd = distanceSqrd;
                nearest = other;

                continue;
            }

            throw new NotImplementedException();
        }

        float distance;
        switch (nearest?.ShapeType)
        {
            case null: return false;
            case Shape.Circle: 
                distance = MathF.Sqrt(nearestDistSqrd.Value);
                collision.Depth = radii - distance;
                collision.Normal = (circle.Center - nearest.Value.Circle.Center) / distance;
                collision.Point = nearest.Value.Circle.Center + (collision.Normal * nearest.Value.Circle.Radius);     
                return true;
            case Shape.Rectangle:
                distance = MathF.Sqrt(nearestDistSqrd.Value);
                collision.Depth = circle.Radius - distance;
                var normal = (circle.Center - closestPointOnRect) / distance;
                collision.Normal = MathF.Abs(normal.X) > MathF.Abs(normal.Y) 
                    ? new Vector2(normal.X, 0)
                    : new Vector2(0, normal.Y);

                collision.Point = circle.Center + (-collision.Normal * (circle.Radius - collision.Depth));
                return true;
            default: throw new NotImplementedException();
        }
    }

    // Returns the x nearest in the provided collision span of size x, ordered
    public static bool Circle(ReadOnlySpan<Entity<Collider2D>> world, Circle circle, ref Span<Collision> collisions, Idx<Collider2D>? self = null)
    {
        float radii = 0;
        float radiiSqrd = 0; 
        float distanceSqrd = 0;

        var collisionCount = 0;

        for (int colliderIdx = 0; colliderIdx < world.Length; colliderIdx++)
        {
            if (self == (Idx<Collider2D>)colliderIdx)  { continue; }

            ref readonly var other = ref world[colliderIdx].Item;
            if (other.ShapeType is Shape.Circle)
            {
                radii = other.Circle.Radius + circle.Radius; 
                radiiSqrd = radii * radii;
                distanceSqrd = Vector2.DistanceSquared(other.Circle.Center, circle.Center);

                if (distanceSqrd > radiiSqrd) { continue; }

                var distance = MathF.Sqrt(distanceSqrd);
                var depth = radii - distance;

                var insertIdx = 0;
                for (insertIdx = 0; insertIdx < collisionCount; insertIdx++)
                {
                    if (depth > collisions[insertIdx].Depth) { break; }
                }

                if (insertIdx >= collisions.Length) { continue; }

                var collision = new Collision() { OtherId = new Idx<Collider2D>(colliderIdx) };
                collision.Depth = depth;
                collision.Normal = (circle.Center - other.Circle.Center) / distance;
                collision.Point = other.Circle.Center + (collision.Normal * other.Circle.Radius);     

                collisions.Slice(0, Math.Min(collisionCount + 1, collisions.Length)).Insert(collision, insertIdx);
                collisionCount = Math.Min(collisionCount + 1, collisions.Length);
            }

            throw new NotImplementedException();
        }

        collisions = collisions.Slice(0, Math.Min(collisionCount, collisions.Length));
        return collisions.Length > 0;
    }

    // only returns if there is an overlap or not
    public static bool Cone(ref readonly ReadOnlySpan<Entity<Collider2D>> world, Cone cone, Idx<Collider2D>? self = null)
    {
        throw new NotImplementedException();
    }

    /// Returns the nearest as in the provided collision object
    public static bool Cone(ref readonly ReadOnlySpan<Entity<Collider2D>> world, Cone cone, ref Collision collision, Idx<Collider2D>? self = null)
    {
        throw new NotImplementedException();
    }

    // Returns the x nearest in the provided collision span of size x, ordered
    public static bool Cone(ReadOnlySpan<Entity<Collider2D>> world, Cone cone, ref Span<Collision> collisions, Idx<Collider2D>? self = null)
    {
        float radii = 0;
        float radiiSqrd = 0; 
        float distanceSqrd = 0;

        float angleCos = MathF.Cos(cone.Angle / 2);
        var coneLength = Vector2.Distance(cone.Tip, cone.Base);
        var coneDir = (cone.Base - cone.Tip) / coneLength;
        var circle = new Circle { Center = cone.Tip, Radius = coneLength };

        var collisionCount = 0;

        for (int colliderIdx = 0; colliderIdx < world.Length; colliderIdx++)
        {
            if (self == (Idx<Collider2D>)colliderIdx)  { continue; }

            ref readonly var other = ref world[colliderIdx].Item;
            if (other.ShapeType is Shape.Circle)
            {
                radii = other.Circle.Radius + circle.Radius; 
                radiiSqrd = radii * radii;
                distanceSqrd = Vector2.DistanceSquared(other.Circle.Center, circle.Center);

                var distance = MathF.Sqrt(distanceSqrd);
                var dir = (other.Circle.Center - circle.Center) / distance;
                var dot = Vector2.Dot(coneDir, dir);

                if (distanceSqrd > radiiSqrd || dot < angleCos) { continue; }

                var insertIdx = 0;
                var depth = radii - distance;

                for (insertIdx = 0; insertIdx < collisionCount; insertIdx++)
                {
                    if (depth > collisions[insertIdx].Depth) { break; }
                }

                if (insertIdx >= collisions.Length) { continue; }

                var collision = new Collision() { OtherId = new Idx<Collider2D>(colliderIdx) };

                collision.Depth = depth;
                collision.Normal = (circle.Center - other.Circle.Center) / distance;
                collision.Point = other.Circle.Center + (collision.Normal * other.Circle.Radius);     

                collisions
                    .Slice(0, Math.Min(collisionCount + 1, collisions.Length))
                    .Insert(collision, insertIdx);

                collisionCount = Math.Min(collisionCount + 1, collisions.Length);
                continue;
            }

            throw new NotImplementedException();
        }

        collisions = collisions.Slice(0, collisionCount);
        return collisions.Length > 0;
    }

    public static bool LineSegment(ReadOnlySpan<Entity<Collider2D>> world, LineSegment segment, ref Collision collision, Idx<Collider2D>? self = null)
    {
        Vector2? closestIntersectionPoint = null;
        float closestDistanceSqrd = float.MaxValue;

        for (int i = 0; i < world.Length; i++)
        {
            if (self == (Idx<Collider2D>)i) { continue; }
            ref readonly var other = ref world[i].Item;

            var intersectionPoint = other.ShapeType switch
            {
                Shape.Circle => Overlap.Circle(segment, other.Circle).Item1,
                Shape.Rectangle => Overlap.Rect(segment, other.Rectangle).Item1,
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
            case Shape.Rectangle:
                var rect = nearest.Rectangle;
                collision.Point = closestIntersectionPoint.Value;
                collision.Depth = Vector2.Distance(closestIntersectionPoint.Value, segment.End);
                collision.Normal = Vector2.Round(Vector2.Normalize(rect.Center - collision.Point));
                return true;
            default: throw new NotImplementedException();
        }
    }

    public static (Vector2?, Vector2?) Circle(LineSegment segment, Circle circle)
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
            Physics2D.InSegmentRange(segment, p1) ? p1 : null,
            Physics2D.InSegmentRange(segment, p2) ? p2 : null
        );
    }

    public static (Vector2?, Vector2?) Rect(LineSegment segment, Rectangle rect)
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

            var intersection = Physics2D.FindIntersection(ls, segment);
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

    public static Vector2 ClosestPointOnLine(LineSegment segment, Vector2 from)
    {
        var segmentDelta = segment.End - segment.Start;
        var t = Vector2.Dot(from - segment.Start, segmentDelta) / segmentDelta.LengthSquared();
        t = float.Clamp(t, 0, 1);

        return segment.Start + (segmentDelta * t);
    }
}