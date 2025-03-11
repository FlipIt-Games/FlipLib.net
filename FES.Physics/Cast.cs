using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace FES.Physics;

public static class Cast
{
    public static bool Collider2D(
        ReadOnlySpan<Entity<Collider2D>> world,
        Collider2D collider, 
        Vector2 movement,
        ref Collision collision, 
        Idx<Collider2D>? self = null
    )
    {
        return collider.ShapeType switch 
        {
            CollisionShape.Circle => Circle(world, collider.Circle, movement, ref collision, self),
            _ => throw new NotImplementedException()
        };
    }

    public static bool Circle(
        ReadOnlySpan<Entity<Collider2D>> world,
        Circle circle, 
        Vector2 movement,
        ref Collision collision, 
        Idx<Collider2D>? self = null
    )
    {
        var dir = Vector2.Normalize(movement);
        var perpendicular = new Vector2(dir.Y, dir.X);

        var ls1Start = circle.Center + (perpendicular * circle.Radius);
        var ls2Start = circle.Center - (perpendicular * circle.Radius);
        var ls1 = new LineSegment { Start = ls1Start, End = ls1Start + (movement)};
        var ls2 = new LineSegment { Start = ls2Start, End = ls2Start + (movement)};

        var endCircle = circle with { Center = circle.Center + movement };

        var ls1Collision = new Collision();
        var ls2Collision = new Collision();
        var endCircleCollision = new Collision();

        var ls1Hit = Overlap.LineSegment(world, ls1, ref ls1Collision, self);
        var ls2Hit = Overlap.LineSegment(world, ls2, ref ls2Collision, self);
        var endCircleHit = Overlap.Circle(world, endCircle, ref endCircleCollision, self);

        if (!ls1Hit && !ls2Hit && !endCircleHit) { return false; }

        var ls1DistanceSqrd = ls1Hit ? Vector2.DistanceSquared(ls1Collision.Point, ls1Start) : float.PositiveInfinity;
        var ls2DistanceSqrd = ls2Hit ? Vector2.DistanceSquared(ls2Collision.Point, ls2Start) : float.PositiveInfinity;
        var endCircleDistanceSqrd = endCircleHit ? Vector2.DistanceSquared(endCircleCollision.Point, circle.Center) : float.PositiveInfinity;

        if (ls1DistanceSqrd <= ls2DistanceSqrd && ls1DistanceSqrd <= endCircleDistanceSqrd)
        {
            collision = ls1Collision;
            return true;
        }

        if (ls2DistanceSqrd <= ls1DistanceSqrd && ls2DistanceSqrd <= endCircleDistanceSqrd)
        {
            collision = ls2Collision;
            return true;
        }

        if (endCircleDistanceSqrd <= ls1DistanceSqrd && endCircleDistanceSqrd <= ls2DistanceSqrd)
        {
            collision = endCircleCollision;
            return true;
        }

        throw new InvalidOperationException();
    }
}