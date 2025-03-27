using System.Numerics;
using System.Runtime.InteropServices;

namespace FlipLib.Physics;

public enum CollisionShape
{
    Circle = 1,
    AABB = 2,
    Cone = 3
}

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct Collider2D
{
    [FieldOffset(0)]
    public Circle Circle; 

    [FieldOffset(0)]
    public Rectangle AABB;

    [FieldOffset(0)]
    public Cone Cone;

    [FieldOffset(24)]
    public CollisionShape ShapeType;
}

public readonly ref struct OverlapQuery<T> where T : unmanaged
{
    public readonly T Entity;
    public readonly Idx<Collider2D>? Self;
    public readonly ReadOnlySpan<Entity<Collider2D>> World;

    public OverlapQuery(T entity, ReadOnlySpan<Entity<Collider2D>> world, Idx<Collider2D>? self = null)
    {
        Entity = entity;
        World = world;
        Self = self;
    }
}

public readonly ref struct CastQuery<T> where T : unmanaged
{
    public readonly T Entity;
    public readonly Vector2 Displacement;
    public readonly Idx<Collider2D>? Self;
    public readonly ReadOnlySpan<Entity<Collider2D>> World;

    public CastQuery(T entity, Idx<Collider2D>? self, Vector2 displacement, ReadOnlySpan<Entity<Collider2D>> world)
    {
        Entity = entity;
        Self = self;
        Displacement = displacement;
        World = world;
    }
}

public struct Collision
{
    public Idx<Collider2D> OtherId;
    public Vector2 Point;
    public Vector2 Normal;
    public float Depth;
}
