using System.Numerics;

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

    public override bool Equals(object? obj)
        => obj is GridCoordinate coord && coord == this;

    public override int GetHashCode()
        => (Column, Row).GetHashCode();
}

public struct PathFindingNode 
{
    public enum NodeState
    {
        Unvisited,
        Open,
        Closed,
        NearestWalkableVisited,
    }

    public Idx<PathFindingNode> ParentIdx;
    public NodeState State;

    public int GValue;
    public int HValue;

    public int FValue => GValue + HValue;
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

    public byte[,] _unwalkablesCells;

    public PathFindingNode[,] _nodes;
    public MinHeap<PathFindingNode> _openList;
    public GridCoordinate[] _nearestWalkableQueue;

    public NavigationGrid(float cellSize, int columns, int rows)
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

        _unwalkablesCells = new byte[columns, rows];

        _nodes = new PathFindingNode[columns, rows];
        _openList = new(_unwalkablesCells.Length);
        _nearestWalkableQueue = new GridCoordinate[columns * rows];
    }

    /// <summary>
    /// Find the nearest path from origin to destination and writes it down the path argument 
    /// while checking for obstacles that belongs to the provided mask
    /// </summary>
    /// <param name="origin">The start position</param>
    /// <param name="destination">The position to reach</param>
    /// <param name="path">The buffer the path will be written in</param>
    /// <param name="mask">The layers to consider while checking for obstacles</param>
    public void FindPath(Vector2 origin, Vector2 destination, ref Path path, byte mask)
    {
        // Clear everything
        path.Clear();
        _openList.Clear();

        // TODO: this is really bad, we should ideally find a way to not have to clear the nodes between calls
        for (int col = 0; col < _nodes.GetLength(0); col++)
        {
            for (int row = 0; row < _nodes.GetLength(1); row++)
            {
                _nodes[col, row] = new() { State = PathFindingNode.NodeState.Unvisited };
            }
        }

        // Set destination cell and correct it if it is overlapping with an unwalkable cell
        var startCell = ToGridCoordinates(origin);
        var endCell = ToGridCoordinates(destination);

        if ((_unwalkablesCells[endCell.Column, endCell.Row] & mask) != 0)
        {
            endCell = FindNearestWalkableCell(endCell, mask); 
        }

        ref var startNode = ref _nodes[startCell.Column, startCell.Row];
        startNode.State = PathFindingNode.NodeState.Open;

        var startNodeIdx = ToIdx(startCell);
        _openList.Insert(startNodeIdx, 0);

        Span<GridCoordinate> neighbours = stackalloc GridCoordinate[8];

        while(_openList.Size > 0)
        {
            var currentIdx = _openList.RemoveFirst();
            var currentCell = ToGridCoord(currentIdx);
            ref var current = ref _nodes[currentCell.Column, currentCell.Row];

            // If we reached the destination we can build the path by walking parents
            if (currentCell == endCell) 
            {
                while(currentCell != startCell)
                {
                    path.PushFront(ToWorldPosition(currentCell));

                    currentCell = ToGridCoord(current.ParentIdx);
                    current = _nodes[currentCell.Column, currentCell.Row];
                }
                return;
            }

            // Performs Line of Sight check to remove redundant nodes, this allows for "any angle" path creation
            // instead of grid constrained path
            if (currentCell != startCell && current.ParentIdx != startNodeIdx)
            {
                var parentCell = ToGridCoord(current.ParentIdx);
                var parent = _nodes[parentCell.Column, parentCell.Row];
                var grandParentCell = ToGridCoord(parent.ParentIdx);

                if (HasLineOfSight(parentCell, grandParentCell, mask))
                {
                    var grandParent = _nodes[grandParentCell.Column, grandParentCell.Row];
                    current.ParentIdx = parent.ParentIdx;
                    current.GValue = grandParent.GValue + GetDistance(grandParentCell, currentCell);
                }
            }

            current.State = PathFindingNode.NodeState.Closed;

            var neighboursCount = GetNeighbours(neighbours, currentCell);
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbourCell = neighbours[i];
                if ((_unwalkablesCells[neighbourCell.Column, neighbourCell.Row] & mask) != 0) { continue; }

                var neighbour = _nodes[neighbourCell.Column, neighbourCell.Row];
                if (neighbour.State == PathFindingNode.NodeState.Closed) 
                { 
                    continue; 
                }

                int cost = current.GValue + GetDistance(neighbourCell, currentCell);
                if (neighbour.State == PathFindingNode.NodeState.Open && cost >= neighbour.GValue) 
                {
                   continue; 
                }

                neighbour.GValue = cost;
                neighbour.HValue = GetDistance(neighbourCell, endCell);
                neighbour.ParentIdx = currentIdx;

                var idx = ToIdx(neighbourCell);
                if (neighbour.State == PathFindingNode.NodeState.Open)
                {
                    _openList.Update(idx, neighbour.FValue);
                }
                else 
                {
                    _openList.Insert(idx, neighbour.FValue);
                    neighbour.State = PathFindingNode.NodeState.Open;
                }

                _nodes[neighbourCell.Column, neighbourCell.Row] = neighbour;
            }
        }
    }

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
                var c = Math.Clamp(col, 0, _unwalkablesCells.GetLength(0) - 1);
                var r = Math.Clamp(row, 0, _unwalkablesCells.GetLength(1) - 1);
                var current = _unwalkablesCells[c, r];
                _unwalkablesCells[c, r] = (byte)(value ? current | mask : current ^ mask);
            }
        }
    }

    public void SetUnwalkable(GridCoordinate coordinates, byte mask, bool value)
    {
        var column = coordinates.Column;
        var row = coordinates.Row;

#if DEBUG
        var rowCount = _unwalkablesCells.GetLength(0);
        var colCount = _unwalkablesCells.GetLength(1);


        if (row < 0 || column < 0 || row >= rowCount || column >= colCount)
        {
            throw new IndexOutOfRangeException($"Cell (x: {row}, y: {column}) is outside of grid. Grid has {rowCount} rows and {colCount} with cell size of {CellSize}");
        }
#endif

        var current = _unwalkablesCells[column, row];
        current = (byte)(value ? current | mask : current ^ mask);
    }

    /// <summary>
    /// Returns the coordinate of the cell containing the provided world position Vector 
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The coordinate of the cell containing the provided world position Vector </returns>
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
    public Vector2 ToWorldPosition(GridCoordinate coordinates)
    {
        return Grid0ToWorld + new Vector2(coordinates.Column * CellSize, -coordinates.Row * CellSize);
    }

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

    public bool ContainsCell(GridCoordinate coordinate)
        => coordinate.Column >= 0 && 
           coordinate.Column < _unwalkablesCells.GetLength(0) &&
           coordinate.Row >= 0 && 
           coordinate.Row < _unwalkablesCells.GetLength(1); 

    public int GetDistance(GridCoordinate a, GridCoordinate b)
    {
        var dx = (int)MathF.Abs(b.Column - a.Column); 
        var dy = (int)MathF.Abs(b.Row - a.Row);
        
        return dx < dy 
            ? dx * DiagonalDistance + ((dy - dx) * OrthogonalDistance)
            : dy * DiagonalDistance + ((dx - dy) * OrthogonalDistance);
    }

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
            if ((_unwalkablesCells[x0, y0] & mask) != 0) // Assuming 1 represents an obstacle
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

    public GridCoordinate ToGridCoord(Idx<PathFindingNode> idx)
    {
        var rowCount = _nodes.GetLength(1);
        return new GridCoordinate(idx.Value / rowCount, idx.Value % rowCount);
    }

    public Idx<PathFindingNode> ToIdx(GridCoordinate coordinates)
    {
        return (Idx<PathFindingNode>)(coordinates.Column * _nodes.GetLength(1) + coordinates.Row);
    }

    public GridCoordinate FindNearestWalkableCell(GridCoordinate destination, byte mask)
    {
        var queueStart = 0;  
        var queueEnd = 0;  

        _nearestWalkableQueue[queueEnd++] = destination;
        _nodes[destination.Column, destination.Row].State = PathFindingNode.NodeState.NearestWalkableVisited;

        Span<(int X, int Y)> directions = stackalloc (int X, int Y)[]
        { 
            (-1, 0), (1, 0), (0, -1), (0, 1), (-1, -1), (-1, 1), (1, -1), (1, 1) 
        };

        while(queueStart < queueEnd)
        {
            var current = _nearestWalkableQueue[queueStart++];
            foreach(var (x, y) in directions)
            {
                var neighbour = current + new GridCoordinate(x, y);
                if (_nodes[neighbour.Column, neighbour.Row].State == PathFindingNode.NodeState.NearestWalkableVisited ||
                    neighbour.Column < 0 || neighbour.Column >= _unwalkablesCells.GetLength(0) ||
                    neighbour.Row < 0 || neighbour.Row >= _unwalkablesCells.GetLength(1))
                {
                    continue;
                }

                if ((_unwalkablesCells[neighbour.Column, neighbour.Row] & mask) == 0) { return neighbour; }

                _nearestWalkableQueue[queueEnd++] = neighbour;
                _nodes[neighbour.Column, neighbour.Row].State = PathFindingNode.NodeState.NearestWalkableVisited;
            }
        }

        return destination;
    }
}
