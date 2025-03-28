using System.Numerics;
using System.Runtime.CompilerServices;

using FlipLib.Physics;

namespace FlipLib.AI;

public struct GridCoordinate
{
    public int Column;
    public int Row;

    public GridCoordinate(int column, int row)
    {
        Column = column;
        Row = row;
    }

    public override string ToString()
        => $"(Column: {Column}, Row: {Row})";

    public static GridCoordinate operator +(GridCoordinate a, GridCoordinate b)
        => new GridCoordinate(a.Column + b.Column, a.Row + b.Row);

    public static GridCoordinate operator -(GridCoordinate a, GridCoordinate b)
        => new GridCoordinate(a.Column - b.Column, a.Row - b.Row);

    public static bool operator ==(GridCoordinate a, GridCoordinate b) 
        => a.Column == b.Column && a.Row == b.Row;

    public static bool operator !=(GridCoordinate a, GridCoordinate b) 
        => a.Column != b.Column || a.Row != b.Row;

    public override bool Equals(object obj)
        => obj is GridCoordinate coord && coord == this;

    public override int GetHashCode()
        => (Column, Row).GetHashCode();
}

public struct NavigationGrid
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
    public readonly GridCoordinate World0ToGrid;

    public int _nearestWalkableGeneration;

    public Memory<byte> Cells;
    public Memory<int> _nearestWalkableQueue;
    public Memory<(bool Visited, int Generation)> _nearestWalkableVisited;

    public NavigationGrid(float cellSize, int columns, int rows, IAllocator allocator = null)
    {
        CellSize = cellSize;
        InverseCellSize = 1 / cellSize;

        WorldWidth = columns * cellSize;
        WorldHeight = rows * cellSize;
        ColumnCount = columns;
        RowCount = rows;

        // assumes the grid is centered at world (0;0)
        World0ToGrid = new((int)(columns * cellSize), (int)(rows * cellSize));
        Grid0ToWorld = new Vector2(-columns * cellSize * 0.5f, rows * cellSize * 0.5f) + new Vector2(cellSize * 0.5f, -cellSize * 0.5f);

        if (allocator is null)
        {
            Cells = new byte[columns * rows];
            _nearestWalkableQueue = new int[columns * rows];
            _nearestWalkableVisited = new (bool Visited, int Generation)[columns * rows];
        }
        else
        {
            Cells = allocator.AllocZeroed<byte>(columns * rows);
            _nearestWalkableQueue = allocator.AllocZeroed<int>(columns * rows);
            _nearestWalkableVisited = allocator.AllocZeroed<(bool Visited, int Generation)>(columns * rows);
        }
    }

    public byte this[int idx]
    {
        get
        {
#if DEBUG
            if ((int)idx >= Cells.Length)
            {
                throw new IndexOutOfRangeException(nameof(idx));
            }
#endif
            return Cells.Span[idx];
        }
        set
        {
 #if DEBUG
            if ((int)idx >= Cells.Length)
            {
                throw new IndexOutOfRangeException(nameof(idx));
            }
#endif           
            Cells.Span[idx] = value;
        }
    }

    public byte this[int col, int row]
    {
        get => this[col * RowCount + row];
        set => this[col * RowCount + row] = value;
    }

    public byte this[GridCoordinate coordinate]
    {
        get => this[coordinate.Column, coordinate.Row];
        set => this[coordinate.Column, coordinate.Row] = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetUnwalkable(ReadOnlySpan<Entity<Collider2D>> world, byte mask, bool value)
    {
        foreach (var entity in world) 
        {
            SetUnwalkable(entity.Item, mask, value);
        }
    }

    public void SetUnwalkable(Collider2D collider, byte mask, bool value)
    {
        var rect = collider.ShapeType switch
        {
            CollisionShape.AABB => collider.AABB,
            CollisionShape.Circle => collider.Circle.GetOuterSquare(),
            _ => throw new NotImplementedException()
        };

        rect = rect with
        {
            HalfWidth = rect.HalfWidth + CellSize,
            HalfHeight = rect.HalfHeight + CellSize
        };

        var firstCell = ToGridCoordinates(rect.GetCornerPosition(RectangleCorner.TopLeft));
        var startCol = firstCell.Column;
        var startRow = firstCell.Row;

        var endCol = startCol + (int)MathF.Ceiling((rect.HalfWidth * 2 * InverseCellSize)) -1;
        var endRow = startRow + (int)MathF.Ceiling((rect.HalfHeight * 2 * InverseCellSize)) -1;

        for (int col = startCol; col <= endCol; col++)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                var c = Math.Clamp(col, 0, ColumnCount - 1);
                var r = Math.Clamp(row, 0, RowCount - 1);
                var current = this[c, r];
                this[c, r] = (byte)(value ? current | mask : current ^ mask);
            }
        }
    }

    public void SetUnwalkable(GridCoordinate coordinates, byte mask, bool value)
    {
        var column = coordinates.Column;
        var row = coordinates.Row;

#if DEBUG
        if (row < 0 || column < 0 || row >= RowCount || column >= ColumnCount)
        {
            throw new IndexOutOfRangeException($"Cell (x: {row}, y: {column}) is outside of grid. Grid has {RowCount} rows and {ColumnCount} with cell size of {CellSize}");
        }
#endif

        var current = this[column, row];
        current = (byte)(value ? current | mask : current ^ mask);
    }

    /// <summary>
    /// Returns the coordinate of the cell containing the provided world position Vector 
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The coordinate of the cell containing the provided world position Vector </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public GridCoordinate ToGridCoordinates(Vector2 position)
    {
        var scaled = new Vector2(position.X * InverseCellSize, -position.Y * InverseCellSize);
        return new(World0ToGrid.Column + (int)MathF.Floor(scaled.X), World0ToGrid.Row + (int)MathF.Floor(scaled.Y));
    }

    /// <summary>
    /// Returns The center of the cell as world position
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>The center of the cell as world position</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToWorldPosition(GridCoordinate coordinates)
    {
        return Grid0ToWorld + new Vector2(coordinates.Column * CellSize, -coordinates.Row * CellSize);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetNeighbours(Span<GridCoordinate> buffer, GridCoordinate coordinate)
    {
        var neighboursCount = 0;
        for (int col = -1; col <= 1; col++)
        {
            for (int row = -1; row <= 1; row++)
            {
                if (row == 0 && col == 0) { continue; }
                var cell = coordinate + new GridCoordinate(col, row);
                if (ContainsCell(cell))
                {
                    buffer[neighboursCount++] = cell;
                }
            }
        }

        return neighboursCount;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsCell(GridCoordinate coordinate)
        => coordinate.Column >= 0 && 
           coordinate.Column < ColumnCount &&
           coordinate.Row >= 0 && 
           coordinate.Row < RowCount; 

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetDistance(GridCoordinate a, GridCoordinate b)
    {
        var dx = (int)MathF.Abs(b.Column - a.Column); 
        var dy = (int)MathF.Abs(b.Row - a.Row);
        
        return dx < dy 
            ? dx * DiagonalDistance + ((dy - dx) * OrthogonalDistance)
            : dy * DiagonalDistance + ((dx - dy) * OrthogonalDistance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasLineOfSight(GridCoordinate start, GridCoordinate end, byte mask)
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
            if ((this[x0, y0] & mask) != 0) // Assuming 1 represents an obstacle
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

    public int To1DIdx(GridCoordinate coordinate)
        => (coordinate.Column * RowCount) + coordinate.Row;

    public int To1DIdx(int col, int row)
        => (col * RowCount) + row;

    public GridCoordinate ToGridCoordinate(int idx)
    {
        var col = Math.DivRem(idx, RowCount, out var row);
        return new GridCoordinate(col, row);
    }

    public GridCoordinate FindNearestWalkableCell(GridCoordinate destination, byte mask)
    {
        _nearestWalkableGeneration++;
        var queueStart = 0;  
        var queueEnd = 0; 

        var cells = Cells.Span;
        var queue = _nearestWalkableQueue.Span;
        var visited = _nearestWalkableVisited.Span;

        var destinationId = To1DIdx(destination);
        queue[queueStart++] = destinationId;
        visited[destinationId] = (true, _nearestWalkableGeneration);

        Span<(int X, int Y)> directions = stackalloc (int X, int Y)[]
        { 
            (-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1) 
        };

        while(queueStart < queueEnd)
        {
            var current = queue[queueStart++];
            foreach(var (x, y) in directions)
            {
                var neighbour = current + To1DIdx(x, y);
                if (visited[neighbour] == (true, _nearestWalkableGeneration) || neighbour < 0 || neighbour >= Cells.Length)
                {
                    continue;
                }

                if ((cells[neighbour] & mask) == 0) { return ToGridCoordinate(neighbour); }

                queue[queueEnd++] = neighbour;
                visited[neighbour] = (true, _nearestWalkableGeneration);
            }
        }

        return destination;
    }
}
