using System.Numerics;
using System.Runtime.CompilerServices;

namespace FlipLib.Physics;

public static partial class Overlap
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (Vector2?, Vector2?) Rect(Rectangle rect, Line line)
        => Line(line, rect);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static partial (Vector2?, Vector2?) Rect(Rectangle rect, LineSegment segment)
        => LineSegment(segment, rect);
}
