using FES;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace FES.Physics;

using Shape = CollisionShape;

public static class Overlap
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
        float distanceSqrd = 0;

        Vector2 closestPointOnRect = Vector2.Zero;

        for (int i = 0; i < world.Length; i++)
        {
            if (self == (Idx<Collider2D>)i)  { continue; }

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

                closestPointOnRect =  new Vector2(
                    Math.Clamp(circle.Center.X, rect.Center.X - halfWidth, rect.Center.X + halfWidth),
                    Math.Clamp(circle.Center.Y, rect.Center.Y - halfHeight, rect.Center.Y + halfHeight)
                );

                distanceSqrd = Vector2.DistanceSquared(circle.Center, closestPointOnRect);

                if (distanceSqrd > circle.Radius * circle.Radius) { continue; }

                collision.OtherId = new Idx<Collider2D>(i);

                nearestDistSqrd = distanceSqrd;
                nearest = other;
                collision.OtherId = new Idx<Collider2D>(i);

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
                distance = MathF.Sqrt(distanceSqrd);
                collision.Depth = circle.Radius - distance;
                collision.Normal = (circle.Center - closestPointOnRect) / distance;
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

                collisions.Slice(0, Math.Min(collisionCount + 1, collisions.Length)).Insert(collision, insertIdx);
                collisionCount = Math.Min(collisionCount + 1, collisions.Length);
                continue;
            }

            throw new NotImplementedException();
        }

        collisions = collisions.Slice(0, collisionCount);
        return collisions.Length > 0;
    }
}