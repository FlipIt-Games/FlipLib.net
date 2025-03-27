using System.Numerics;

namespace FlipLib.Physics;

public static class Cast
{
    public static bool Circle(ref readonly CastQuery<Circle> query, ref Collision collision)
    {
        var circle = query.Entity;
        var self = query.Self;
        var movement = query.Displacement;
        var world = query.World;

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

        var l1Query = new OverlapQuery<LineSegment>(ls1, world, self);
        var l2Query = new OverlapQuery<LineSegment>(ls2, world, self);
        var circleQuery = new OverlapQuery<Circle>(endCircle, world, self);

        var ls1Hit = Overlap.LineSegment(ref l1Query, ref ls1Collision);
        var ls2Hit = Overlap.LineSegment(ref l2Query, ref ls2Collision);
        var endCircleHit = Overlap.Circle(ref circleQuery, ref endCircleCollision);

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
