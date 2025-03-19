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

    public Rectangle GetOuterSquare()
        => new Rectangle 
        {
            Center = Center,
            Width = Radius * 2,
            Height = Radius * 2
        };

    public Rectangle GetInnerSquare()
    {
        var size = MathF.Sqrt(Radius * Radius * 0.5f);
        return new Rectangle 
        {
            Center = Center,
            Width = size,
            Height = size,
        };
    }
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

public enum RectangleEdge 
{ 
    Top,
    Bottom,
    Left,
    Right
}

public enum RectangleCorner
{
    TopLeft,
    TopRight,
    BottomRight,
    BottomLeft
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

    public Vector2 GetCornerPosition(RectangleCorner corner)
    {
        var halfWidth = Width / 2;
        var halfHeight = Height / 2;

        return corner switch
        {
            RectangleCorner.TopLeft => Center + new Vector2(-halfWidth, halfHeight),
            RectangleCorner.TopRight => Center + new Vector2(halfWidth, halfHeight),
            RectangleCorner.BottomLeft => Center + new Vector2(halfWidth, -halfHeight),
            RectangleCorner.BottomRight => Center + new Vector2(-halfWidth, -halfHeight),
            _ => throw new InvalidOperationException()
        };
    }

    public RectangleCorner GetClosestCorner(Vector2 point)
    {
        throw new NotImplementedException();
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

    public RectangleEdge GetClosestRectangleEdge(Vector2 point)
    {
        var normal = GetClosestEdgeNormal(point);
        return (normal.X, normal.Y) switch
        {
            (0, 1) => RectangleEdge.Top,
            (0, -1) => RectangleEdge.Bottom,
            (1, 0) => RectangleEdge.Right,
            (-1, 0) => RectangleEdge.Left,
            _ => throw new InvalidOperationException($"normal: {normal} is not an edge normal")
        };
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