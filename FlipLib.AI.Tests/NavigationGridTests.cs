using System.Numerics;
using FES.Physics;

namespace FES.AI.Tests;

[TestFixture]
public class NavigationGridTests
{
    private static NavigationGrid testGrid = new NavigationGrid(0.5f, 20, 20);

    private static IEnumerable<TestCaseData> ToWorldPositionTestCases
    {
        get
        {
            yield return new TestCaseData(new GridCoord(0, 0), new Vector2(-4.75f, 4.75f));
            yield return new TestCaseData(new GridCoord(19, 19), new Vector2(4.75f, -4.75f));
            yield return new TestCaseData(new GridCoord(10, 10), new Vector2(0.25f, -0.25f));
            yield return new TestCaseData(new GridCoord(3, 14), new Vector2(-3.25f, -2.25f));
        }
    }

    [Test, TestCaseSource(nameof(ToWorldPositionTestCases))]
    public void ToWorldPosition_ShouldReturnCenterOfGridCell(GridCoord input, Vector2 expected)
    {
        var actual = testGrid.ToWorldPosition(input);
        Assert.That(actual.X, Is.EqualTo(expected.X).Within(0.00001f));
        Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(0.00001f));
    }

    private static IEnumerable<TestCaseData> ToGridCoordinatesTestCases
    {
        get
        {
            yield return new TestCaseData(new Vector2(-5, 5f), new GridCoord(0, 0));
            yield return new TestCaseData(new Vector2(-4.7f, 4.8f), new GridCoord(0, 0));
            yield return new TestCaseData(new Vector2(4.7f, -4.7f), new GridCoord(19, 19));
            yield return new TestCaseData(new Vector2(4.999f, -4.999f), new GridCoord(19, 19));
            yield return new TestCaseData(new Vector2(0, 0), new GridCoord(10, 10));
            yield return new TestCaseData(new Vector2(-3.13f, -2.34f), new GridCoord(3, 14));
        }
    }

    [Test, TestCaseSource(nameof(ToGridCoordinatesTestCases))]
    public void ToGridCoordinates(Vector2 input, GridCoord expected)
    {
        var actual = testGrid.ToGridCoordinates(input);
        Assert.That(actual, Is.EqualTo(expected));
    }

    [Test]
    public void Bite()
    {
        var grid = new NavigationGrid(0.5f, 10, 10);
        int[,] expectedGrid =
        {
            //0  1  2  3  4  5  6  7  8  9
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 0
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 1
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 2
            { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0 }, // 3
            { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0 }, // 4
            { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0 }, // 5
            { 0, 0, 0, 1, 1, 1, 1, 0, 0, 0 }, // 6
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 7
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 8
            { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, // 9
        };

        grid.SetUnwalkable(new Collider2D
        {
            ShapeType = CollisionShape.Rectangle,
            Rectangle = new Rectangle 
            { 
                Center = new Vector2(0, 0),
                Width = 1, 
                Height = 1
            }
        }, 
        1,
        true);

        for (int col = 0; col < 10; col++)
        {
            for (int row = 0; row < 10; row++)
            {
                var expected = expectedGrid[col, row];
                Assert.That(grid._unwalkablesCells[col, row], Is.EqualTo(expected), $"expected grid at {col}, {row} to be {expected}");
            }
        }
    }
}