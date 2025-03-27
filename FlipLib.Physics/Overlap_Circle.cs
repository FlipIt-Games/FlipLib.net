using System.Numerics;
using System.Runtime.CompilerServices;

using Shape = FlipLib.Physics.CollisionShape;

namespace FlipLib.Physics;

public static partial class Overlap
{
    public static partial bool Circle(ref readonly OverlapQuery<Circle> query, ref Collision collision)
    {
        var circle = query.Entity;
        var self = query.Self;
        var world = query.World;

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
            else if (other.ShapeType is Shape.AABB)
            {
                ref readonly var rect = ref other.AABB;
                var closestPointOnRectTmp =  new Vector2(
                    Math.Clamp(circle.Center.X, rect.Center.X - rect.HalfWidth, rect.Center.X + rect.HalfWidth),
                    Math.Clamp(circle.Center.Y, rect.Center.Y - rect.HalfHeight, rect.Center.Y + rect.HalfHeight)
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
            case Shape.AABB:
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

    public static partial bool Circle(ref readonly OverlapQuery<Circle> query, ref Span<Collision> collisions)
    {
        var circle = query.Entity;
        var self = query.Self;
        var world = query.World;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (Vector2?, Vector2?) Circle(Circle circle, LineSegment segment)
        => LineSegment(segment, circle);
}
