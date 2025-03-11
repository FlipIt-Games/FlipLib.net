using FES.Physics;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using Shape = FES.Physics.CollisionShape;

namespace FES.AI;

public static class Navigate
{
    public static void SetPath(
        ReadOnlySpan<Entity<Collider2D>> world,
        ref Path path,
        Collider2D agent,
        Vector2 destination
    )
    {
        path.Clear();

        var startPosition = agent.ShapeType switch
        {
            Shape.Circle => agent.Circle.Center,
            _ => throw new NotImplementedException()
        };

        Span<Vector2> rectCorners = stackalloc Vector2[4];

        var collision = new Collision();
        while(true)
        {
            if (!CastCollider(world, ref path))
            {
                var idx = path.PushBack(destination);
                break;
            }

            var obstacle = world[collision.OtherId.Value].Item;
            if (obstacle.ShapeType is Shape.Rectangle or Shape.Circle)
            {
                var expanded = obstacle.ShapeType == Shape.Rectangle
                    ? obstacle.Rectangle with 
                    { 
                        Width = obstacle.Rectangle.Width + (agent.Circle.Radius * 2.2f),
                        Height = obstacle.Rectangle.Height + (agent.Circle.Radius * 2.2f),
                    }
                    : new Rectangle 
                    {
                        Center = obstacle.Circle.Center,
                        Width = (obstacle.Circle.Radius * 2) + (agent.Circle.Radius * 2.2f),
                        Height = (obstacle.Circle.Radius * 2) + (agent.Circle.Radius * 2.2f),
                    };

                expanded.GetCorners(rectCorners);
                Vector2 closestToCollisionPoint = rectCorners[0];
                float distanceFromCollisionPointSqrd = float.MaxValue;

                for (int i = 0; i < rectCorners.Length; i++)
                {
                    var corner = rectCorners[i];
                    var distanceSqrd = Vector2.DistanceSquared(corner, collision.Point);
                    if (distanceSqrd < distanceFromCollisionPointSqrd)
                    {
                        closestToCollisionPoint = corner;
                    }
                }

                path.PushBack(closestToCollisionPoint);
                startPosition = closestToCollisionPoint;
            }

            bool CastCollider(ReadOnlySpan<Entity<Collider2D>> world, ref readonly Path path)
            {
                switch (agent.ShapeType)
                {
                    case Shape.Circle:
                        Circle circle;
                        var last = path.GetLast();
                        if (last.HasValue)
                        {
                            circle = agent.Circle with { Center = last.Value.Position };
                        }
                        else { circle = agent.Circle; }
                        return Cast.Circle(world, circle, destination - agent.Circle.Center, ref collision);
                    default: throw new NotImplementedException();
                }
            }
        }
    }

    public static void SetSubPath(
        ReadOnlySpan<Entity<Collider2D>> world,
        ref Path path,
        PathNode node,
        Collider2D agent,
        Vector2 destination
    )
    {
        
    }
}