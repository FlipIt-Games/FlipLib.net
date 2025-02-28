using System.Runtime.InteropServices;
using FES;

namespace FES.Physics;

public enum CollisionShape
{
    Circle = 1,
    Rectangle = 2,
    Square = 3,
    Cone = 4
}

[StructLayout(LayoutKind.Explicit, Size = 40)]
public struct Collider2D
{
    [FieldOffset(0)]
    public CollisionShape ShapeType;

    [FieldOffset(8)]
    public Circle Circle; 

    [FieldOffset(8)]
    public Rectangle Rectangle;

    [FieldOffset(8)]
    public Square Square;

    [FieldOffset(8)]
    public Cone Cone;
}