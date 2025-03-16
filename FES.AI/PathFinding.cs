using System.Collections;
using System.Numerics;

using FES.Physics;

namespace FES.AI;

public struct GridCoord
{
    public int Column;
    public int Row;

    public GridCoord(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public override string ToString()
        => $"(Column: {Column}, Row: {Row})";

    public static GridCoord operator +(GridCoord a, GridCoord b)
        => new GridCoord(a.Column + b.Column, a.Row + b.Row);

    public static GridCoord operator -(GridCoord a, GridCoord b)
        => new GridCoord(a.Column - b.Column, a.Row - b.Row);

    public static bool operator ==(GridCoord a, GridCoord b) 
        => a.Column == b.Column && a.Row == b.Row;

    public static bool operator !=(GridCoord a, GridCoord b) 
        => a.Column != b.Column || a.Row != b.Row;

    public override bool Equals(object? obj)
        => obj is GridCoord coord && coord == this;

    public override int GetHashCode()
        => (Column, Row).GetHashCode();
}

public struct AStarNode 
{
    public GridCoord Coordinates;
    public GridCoord ParentCoordinates;

    public bool Closed;
    public bool Open;

    public int GValue;
    public int HValue;

    public int FValue => GValue + HValue;
}

public struct NavGrid
{
    public const int DiagonalDistance = 14;
    public const int OrthogonalDistance = 10;

    public readonly float CellSize;
    public readonly float InverseCellSize;

    public readonly float WorldWidth;
    public readonly float WorldHeight;
    public readonly int ColumnCount;
    public readonly int RowCount;

    public readonly Vector2 Grid0ToWorld;
    public readonly GridCoord World0ToGrid;

    public bool[,] _unwalkablesCells;

    public AStarNode[,] _nodes;
    public MinHeap<AStarNode> _openList;
    public HashSet<Idx<AStarNode>> _closedList;

    public NavGrid(float cellSize, int columns, int rows)
    {
        CellSize = cellSize;
        InverseCellSize = 1 / cellSize;

        WorldWidth = columns * cellSize;
        WorldHeight = rows * cellSize;
        ColumnCount = columns;
        RowCount = rows;

        // assumes the grid is centered at world (0;0)
        World0ToGrid = new((int)(columns * cellSize), (int)(rows * cellSize));
        Grid0ToWorld = new(-columns * cellSize * 0.5f, rows * cellSize * 0.5f);

        _unwalkablesCells = new bool[columns, rows];

        _nodes = new AStarNode[columns, rows];

        _openList = new(_unwalkablesCells.Length);
        _closedList = new(_unwalkablesCells.Length);
    }

    public void SetUnwalkable(ReadOnlySpan<Entity<Collider2D>> world, bool unwalkable)
    {
        foreach (var entity in world) 
        {
            SetUnwalkable(entity.Item, unwalkable);
        }
    }

    public void SetUnwalkable(Collider2D collider, bool unwalkable)
    {
        var rect = collider.ShapeType switch
        {
            CollisionShape.Rectangle => collider.Rectangle,
            CollisionShape.Circle => collider.Circle.GetOuterSquare(),
            _ => throw new NotImplementedException()
        };

        rect = rect with
        {
            Width = rect.Width + (CellSize * 2),
            Height = rect.Height + (CellSize * 2)
        };

        var firstCell = ToGridCoordinates(rect.GetCornerPosition(RectangleCorner.TopLeft));
        var startCol = firstCell.Column;
        var startRow = firstCell.Row;

        var endCol = startCol + (int)MathF.Ceiling((rect.Width * InverseCellSize)) -1;
        var endRow = startRow + (int)MathF.Ceiling((rect.Height * InverseCellSize)) -1;

        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                _unwalkablesCells[col, row] = unwalkable;
            }
        }
    }

    public void SetUnwalkable(Vector2 worldPosition, bool unwalkable)
    {

    }

    public void SetUnwalkable(int row, int column, bool unwalkable)
    {
#if DEBUG
        var rowCount = _unwalkablesCells.GetLength(0);
        var colCount = _unwalkablesCells.GetLength(1);

        if (row < 0 || column < 0 || row >= rowCount || column >= colCount)
        {
            throw new IndexOutOfRangeException($"Cell (x: {row}, y: {column}) is outside of grid. Grid has {rowCount} rows and {colCount} with cell size of {CellSize}");
        }
#endif

        _unwalkablesCells[row, column] = unwalkable;
    }

    public GridCoord ToGridCoordinates(Vector2 position)
    {
        var scaled = new Vector2(position.X * InverseCellSize, -position.Y * InverseCellSize);
        return new(World0ToGrid.Column + (int)MathF.Floor(scaled.X), World0ToGrid.Row + (int)MathF.Floor(scaled.Y));
    }

    public Vector2 ToWorldPosition(GridCoord coordinates)
    {
        return Grid0ToWorld + new Vector2(coordinates.Column * CellSize, -coordinates.Row * CellSize);
    }

    public int GetNeighbours(Span<GridCoord> buffer, GridCoord coordinate)
    {
        var neighboursCount = 0;
        for (int col = -1; col <= 1; col++)
        {
            for (int row = -1; row <= 1; row++)
            {
                if (row == 0 && col == 0) { continue; }
                var cell = coordinate + new GridCoord(col, row);
                if (ContainsCell(cell))
                {
                    buffer[neighboursCount++] = cell;
                }
            }
        }

        return neighboursCount;
    }

    public bool ContainsCell(GridCoord coordinate)
        => coordinate.Column >= 0 && 
           coordinate.Column < _unwalkablesCells.GetLength(0) &&
           coordinate.Row >= 0 && 
           coordinate.Row < _unwalkablesCells.GetLength(1); 

    public void FindPath(Vector2 origin, Vector2 destination, ref Path path)
    {
        path.Clear();

        var startCell = ToGridCoordinates(origin);
        var endCell = ToGridCoordinates(destination);

        ref var startNode = ref _nodes[startCell.Column, startCell.Row];

        Span<GridCoord> neighbours = stackalloc GridCoord[8];

        var iter = 0;
        while(_openList.Size > 0)
        {
            var currentIdx = _openList.RemoveFirst().DataIdx;
            ref var current = ref _nodes[currentIdx.Value / _nodes.GetLength(0), currentIdx.Value % _nodes.GetLength(1)];

            if (current.Coordinates == endCell) 
            {
                ref var node = ref current;
                path.PushFront(ToWorldPosition(endCell));
                while(current.Coordinates != startCell)
                {
                    path.PushFront(ToWorldPosition(current.Coordinates));
                    current = ref _nodes[current.ParentCoordinates.Column, current.ParentCoordinates.Row];
                }

                return;
            }

            if (iter++ >= 2)
            {
                ref readonly var parent = ref _nodes[current.ParentCoordinates.Column, current.ParentCoordinates.Row];
                ref readonly var grandParent = ref _nodes[parent.ParentCoordinates.Column, current.ParentCoordinates.Row];

                if (HasLineOfSight(grandParent.Coordinates, current.Coordinates)) 
                {
                    current.ParentCoordinates = grandParent.Coordinates;
                    current.GValue = grandParent.GValue + GetDistance(grandParent.Coordinates, current.Coordinates);
                }
            }
            
            current.Closed = true;

            var neighboursCount = GetNeighbours(neighbours, current.Coordinates);
            for (int i = 0; i < neighboursCount; i++)
            {
                var cell = neighbours[i];
                if (_unwalkablesCells[cell.Column, cell.Row]) { continue; }

                ref var neighbour = ref _nodes[cell.Column, cell.Row];
                if (neighbour.Closed) { continue; }

                int cost = current.GValue + GetDistance(cell, current.Coordinates);
                if (neighbour.Open && cost >= neighbour.GValue) 
                {
                   continue; 
                }

                neighbour.GValue = cost;
                neighbour.HValue = GetDistance(cell, endCell);
                neighbour.ParentCoordinates = current.Coordinates;

                var idx = new Idx<AStarNode>((_nodes.GetLength(1) * cell.Column) + cell.Row);
                if (neighbour.Open)
                {
                    _openList.Update(idx, neighbour.FValue);
                }
                else 
                {
                    _openList.Insert(idx, neighbour.FValue);
                    neighbour.Open = true;
                }
            }
        }
    }

    public int GetDistance(GridCoord a, GridCoord b)
    {
        var dx = b.Column - a.Column; 
        var dy = b.Row - a.Row;
        
        return dx < dy 
            ? dx * DiagonalDistance + ((dy - dx) * OrthogonalDistance)
            : dy * DiagonalDistance + ((dx - dy) * OrthogonalDistance);
    }

    public bool HasLineOfSight(GridCoord start, GridCoord end)
    {
        var x0 = start.Column;
        var y0 = start.Row;
        var x1 = end.Column;
        var y1 = end.Row;
    
        var dx = Math.Abs(x1 - x0);
        var dy = Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var err = dx - dy;
    
        while (true)
        {
            if (_unwalkablesCells[x0, y0]) // Assuming 1 represents an obstacle
                return false;
    
            if (x0 == x1 && y0 == y1)
                return true;
    
            var e2 = 2 * err;
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}