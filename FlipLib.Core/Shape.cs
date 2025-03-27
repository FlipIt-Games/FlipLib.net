using System.Runtime.InteropServices;
using System.Numerics;

namespace FlipLib;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public record struct LineSegment 
{
    [FieldOffset(0)]
    public Vector2 Start;
    [FieldOffset(8)]
    public Vector2 End;
    
    /// <summary>
    /// Checks whether the provided point sits in the rect bounding the line segment
    /// </summary>
    /// <param name="s"></param>
    /// <param name="p"></param>
    /// <returns>True if the point sits in the rect bounding the line segment</returns>
    public bool IsInRange(Vector2 p)
        => p.X >= Math.Min(Start.X, End.X) 
        && p.X <= Math.Max(Start.X, End.X)
        && p.Y >= Math.Min(Start.Y, End.Y) 
        && p.Y <= Math.Max(Start.Y, End.Y);

    /// <summary>
    /// Get the closest point on the line from the provided point
    /// </summary>
    /// <param name="point">The point</param>
    /// <returns>The closest point on the line from the provided point</returns>
    public Vector2 ClosestPointTo(Vector2 point)
    {
        var segmentDelta = End - Start;
        var t = Vector2.Dot(point - Start, segmentDelta) / segmentDelta.LengthSquared();
        t = Math.Clamp(t, 0, 1);

        return Start + (segmentDelta * t);
    }
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public record struct Ray
{
    [FieldOffset(0)]
    public Vector2 Origin;
    [FieldOffset(8)]
    public Vector2 Direction;
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
public record struct Line
{
    [FieldOffset(0)]
    public Vector2 Point;
    [FieldOffset(8)]
    public Vector2 Direction;
}


[StructLayout(LayoutKind.Explicit, Size = 16)]
public record struct Circle
{
    [FieldOffset(0)]
    public Vector2 Center;
    [FieldOffset(8)]
    public float Radius;

    /// <summary>
    /// Create the smallest rectangle that bounds the circle
    /// </summary>
    /// <returns>The square bounding the circle</returns>
    public Rectangle GetOuterSquare()
        => new Rectangle 
        {
            Center = Center,
            HalfWidth = Radius,
            HalfHeight = Radius
        };

    /// <summary>
    /// Create the largest rectangle that sits in the circle
    /// </summary>
    /// <returns>The largest square that sits in the circle</returns>
    public Rectangle GetInnerSquare()
    {
        var size = MathF.Sqrt(Radius * Radius * 0.5f);
        return new Rectangle 
        {
            Center = Center,
            HalfWidth = size,
            HalfHeight = size,
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
    public float HalfWidth;
    [FieldOffset(12)]
    public float HalfHeight;
    
    /// <summary>
    /// Writes the corners of the rect into the provided span. Corners are stored in the order TopLeft - TopRight - BottomLeft - BottomRight
    /// </summary>
    /// <param name="corners"></param>
    public void GetCorners(Span<Vector2> corners)
    {
        corners[0] = Center + new Vector2(-HalfWidth, HalfHeight);  // Top Left
        corners[1] = Center + new Vector2(HalfWidth, HalfHeight);   // Top Right
        corners[2] = Center + new Vector2(HalfWidth, -HalfHeight);  // Bottom Left
        corners[3] = Center + new Vector2(-HalfWidth, -HalfHeight); // Bottom Right
    }

    /// <summary>
    /// Gets the world position of the corner
    /// </summary>
    /// <param name="corner"></param>
    /// <returns>The corner in world position</returns>
    public Vector2 GetCornerPosition(RectangleCorner corner)
    {
        return corner switch
        {
            RectangleCorner.TopLeft => Center + new Vector2(-HalfWidth, HalfHeight),
            RectangleCorner.TopRight => Center + new Vector2(HalfWidth, HalfHeight),
            RectangleCorner.BottomLeft => Center + new Vector2(HalfWidth, -HalfHeight),
            RectangleCorner.BottomRight => Center + new Vector2(-HalfWidth, -HalfHeight),
            _ => throw new InvalidOperationException()
        };
    }

    public RectangleCorner GetClosestCorner(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public Vector2 GetClosestEdgeNormal(Vector2 point)
    {
        var left = Center.X - HalfWidth;
        var right = Center.X + HalfWidth;
        var bottom = Center.Y - HalfHeight;
        var top = Center.Y + HalfHeight;

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
