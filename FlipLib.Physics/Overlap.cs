using System.Numerics;

namespace FlipLib.Physics;

public static partial class Overlap
{
    public static partial bool Circle(ref readonly OverlapQuery<Circle> query, ref Collision collision);
    public static partial bool Circle(ref readonly OverlapQuery<Circle> query, ref Span<Collision> collisions);
    public static partial (Vector2?, Vector2?) Circle(Circle circle, LineSegment segment);

    public static partial bool Cone(OverlapQuery<Cone> query, ref Span<Collision> collisions);

    public static partial (Vector2?, Vector2?) Line(Line line, Rectangle rect);
    public static partial Vector2? Line(Line line, LineSegment segment);

    public static partial bool LineSegment(ref readonly OverlapQuery<LineSegment> query, ref Collision collision);
    public static partial (Vector2?, Vector2?) LineSegment(LineSegment segment, Circle circle); 
    public static partial (Vector2?, Vector2?) LineSegment(LineSegment segment, Rectangle rect); 
    public static partial Vector2? LineSegment(LineSegment s, Line l);
    public static partial Vector2? LineSegment(LineSegment a, LineSegment b);
    public static partial bool LineSegment(LineSegment s, Vector2 p);

    public static partial (Vector2?, Vector2?) Rect(Rectangle rect, Line line);
    public static partial (Vector2?, Vector2?) Rect(Rectangle rect, LineSegment segment);
}
