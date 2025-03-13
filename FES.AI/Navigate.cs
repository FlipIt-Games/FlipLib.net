using FES.Physics;
using System.Numerics;

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

        Span<Vector2> rectCorners = stackalloc Vector2[4];

        var collision = new Collision();
        var startPosition = agent.ShapeType switch
        {
            Shape.Circle => agent.Circle.Center,
            _ => throw new NotImplementedException()
        };

        while (true)
        {
            if (!CastCollider(world, ref path))
            {
                var idx = path.Push(destination);
                break;
            }

            var obstacleEntity = world[collision.OtherId.Value];
            var obstacle = obstacleEntity.Item;
            if (obstacle.ShapeType is Shape.Rectangle or Shape.Circle)
            {
                // This still throws an exception when player sits inside the bounding box in the case of a circle
                // This still needs a bit of thinking
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

                var intersections = Overlap.Line(expanded, new Line
                {
                    Direction = Vector2.Normalize(destination - startPosition),
                    Point = collision.Point
                });

                var normalsAreParallel = false;
                var normal1 = expanded.GetClosestEdgeNormal(intersections.Item1!.Value);
                if (intersections.Item2.HasValue)
                {
                    var normal2 = expanded.GetClosestEdgeNormal(intersections.Item2.Value);
                    if (normal2 == -normal1)
                    {
                        normalsAreParallel = true; 
                    }
                }

                expanded.GetCorners(rectCorners);

                Vector2 closest = rectCorners[0];
                Vector2 secondClosest = rectCorners[0];

                float closestDistanceSqrd = float.MaxValue;
                float secondClosestDistanceSqrd = float.MaxValue;

                for (int i = 0; i < rectCorners.Length; i++)
                {
                    var corner = rectCorners[i];
                    var distanceSqrd = Vector2.DistanceSquared(corner, collision.Point);
                    if (distanceSqrd < closestDistanceSqrd)
                    {
                        secondClosest = closest;
                        secondClosestDistanceSqrd = closestDistanceSqrd; 
                        closest = corner;
                        closestDistanceSqrd = distanceSqrd;
                    }
                    else if (distanceSqrd < secondClosestDistanceSqrd)
                    {
                        secondClosest = corner;
                        secondClosestDistanceSqrd = distanceSqrd;
                    }
                    else if (distanceSqrd == secondClosestDistanceSqrd)
                    {
                        if (Vector2.DistanceSquared(corner, destination) < Vector2.DistanceSquared(secondClosest, destination)) 
                        { 
                            secondClosest = corner;
                            secondClosestDistanceSqrd = distanceSqrd;
                        }
                    }
                }

                path.Push(closest);
                if (normalsAreParallel)
                {
                    path.Push(secondClosest);
                }

                startPosition = path.GetLast()!.Value.Position;
            }
        }

        bool CastCollider(ReadOnlySpan<Entity<Collider2D>> world, ref readonly Path path)
        {
            switch (agent.ShapeType)
            {
                case Shape.Circle:
                    var circle = agent.Circle with { Center = startPosition };
                    return Cast.Circle(world, circle, destination - agent.Circle.Center, ref collision);
                default: throw new NotImplementedException();
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