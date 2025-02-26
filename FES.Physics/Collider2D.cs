using System.Runtime.InteropServices;

namespace FES.Physics;

public enum CollisionShape
{
    Circle = 1,
    Rectangle = 2,
    Square = 3
}

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct Collider2D
{
    [FieldOffset(0)]
    public CollisionShape ShapeType;

    [FieldOffset(16)]
    public Circle Circle; 

    [FieldOffset(16)]
    public Rectangle Rectangle;

    [FieldOffset(16)]
    public Square Square;
}
