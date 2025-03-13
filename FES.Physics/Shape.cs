using System.Runtime.InteropServices;
using System.Numerics;

namespace FES.Physics;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Circle
{
    [FieldOffset(0)]
    public Vector2 Center;
    [FieldOffset(8)]
    public float Radius;
}

[StructLayout(LayoutKind.Explicit, Size = 32)]
public struct Cone
{
    [FieldOffset(0)]
    public Vector2 Base;

    [FieldOffset(8)]
    public Vector2 Tip;

    [FieldOffset(16)]
    public float Angle;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Rectangle
{
    [FieldOffset(0)]
    public Vector2 Center;
    [FieldOffset(8)]
    public float Width;
    [FieldOffset(12)]
    public float Height;

    public void GetCorners(Span<Vector2> corners)
    {   
        var halfWidth = Width / 2;
        var halfHeight = Height / 2;

        corners[0] = Center + new Vector2(-halfWidth, halfHeight);  // Top Left
        corners[1] = Center + new Vector2(halfWidth, halfHeight);   // Top Right
        corners[2] = Center + new Vector2(halfWidth, -halfHeight);  // Bottom Left
        corners[3] = Center + new Vector2(-halfWidth, -halfHeight); // Bottom Right
    }

    public Vector2 GetClosestEdgeNormal(Vector2 point)
    {
        var halfWidth = Width / 2;
        var halfHeight = Height / 2;

        var left = Center.X - halfWidth;
        var right = Center.X + halfWidth;
        var bottom = Center.Y - halfHeight;
        var top = Center.Y + halfHeight;

        var leftDistance = Math.Abs(point.X - left);
        var rightDistance = Math.Abs(point.X - right);
        var bottomDistance = Math.Abs(point.Y - bottom);
        var topDistance = Math.Abs(point.Y - top);

        var minDistance = Math.Min(Math.Min(leftDistance, rightDistance), Math.Min(bottomDistance, topDistance));

        if (minDistance == leftDistance) { return new Vector2(-1, 0); }
        if (minDistance == rightDistance) { return new Vector2(1, 0); }
        if (minDistance == bottomDistance) { return new Vector2(0, -1); }

        return new Vector2(0, 1);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Square
{
    [FieldOffset(0)]
    public Vector2 Center;
    [FieldOffset(8)]
    public float Size;

    public static implicit operator Rectangle(Square square) 
        => new Rectangle 
        { 
            Center = square.Center,
            Width = square.Size,
            Height = square.Size
        };
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct LineSegment 
{
    [FieldOffset(0)]
    public Vector2 Start;
    [FieldOffset(8)]
    public Vector2 End;

    public override string ToString()
        => $"({Start}, {End})";
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Ray
{
    [FieldOffset(0)]
    public Vector2 Origin;
    [FieldOffset(8)]
    public Vector2 Direction;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct Line
{
    [FieldOffset(0)]
    public Vector2 Point;
    [FieldOffset(8)]
    public Vector2 Direction;
}