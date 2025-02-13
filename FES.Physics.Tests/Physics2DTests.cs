using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace FES.Physics.Tests;

[TestFixture]
public class Physics2DTests
{
    private static object[] OverlapsLineSegmentPointSource = new object[]
    {
        // Diagonal line
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(1, 1), true },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(1.567f, 1.567f), true },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(0, 0), true },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(2, 2), true },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(-0.0001f, -0.0001f), false },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(2.0001f, 2.0001f), false },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(0, 1), false },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(-5, 6), false },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(1, 0), false },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(-1, -1), false },
        new object[] { new LineSegment { Start = new(0, 0), End = new (2, 2)}, new Vector2(3, 3), false },

        // Vertical Line
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 3), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 2.8905f), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 2), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 5), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 1.9999f), false },
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 5.0001f), false },
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 0), false },
        new object[] { new LineSegment { Start = new(1, 2), End = new (1, 5)}, new Vector2(1, 6), false },

        // Horizontal Line
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(3, 2), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(3.4321f, 2), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(1, 2), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(5, 2), true },
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(0.9999f, 2), false },
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(1, 2.0001f), false },
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(1, 1.9999f), false },
        new object[] { new LineSegment { Start = new(1, 2), End = new (5, 2)}, new Vector2(5.0001f, 2), false },
    };

    [TestCaseSource(nameof(OverlapsLineSegmentPointSource))]
    public void Overlaps_LineSegment_Point(LineSegment s, Vector2 p, bool expected)
    {
        var actual = Physics2D.Overlaps(s, p);
        Assert.AreEqual(expected, actual);
    }
}
