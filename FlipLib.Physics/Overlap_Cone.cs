using System.Numerics;

using Shape = FlipLib.Physics.CollisionShape;

namespace FlipLib.Physics;

public static partial class Overlap
{
    // Returns the x nearest in the provided collision span of size x, ordered
    public static partial bool Cone(OverlapQuery<Cone> query, ref Span<Collision> collisions)
    {
        var cone = query.Entity;
        var self = query.Self;
        var world = query.World;

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
}
