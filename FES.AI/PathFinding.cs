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
    public readonly GridCoord World0ToGrid;

    public bool[,] _unwalkablesCells;

    public PathFindingNode[,] _nodes;
    public MinHeap<PathFindingNode> _openList;
    public GridCoord[] _nearestWalkableQueue;

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

        _unwalkablesCells = new bool[columns, rows];

        _nodes = new PathFindingNode[columns, rows];
        _openList = new(_unwalkablesCells.Length);
        _nearestWalkableQueue = new GridCoord[columns * rows];
    }

    public void FindPath(Vector2 origin, Vector2 destination, ref Path path)
    {
        // Clear everything
        path.Clear();
        _openList.Clear();

        // TODO: this is bad, we should ideally find a way to not have to clear the nodes between calls
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

        if (_unwalkablesCells[endCell.Column, endCell.Row])
        {
            endCell = FindNearestWalkableCell(endCell); 
        }

        ref var startNode = ref _nodes[startCell.Column, startCell.Row];
        startNode.State = PathFindingNode.NodeState.Open;

        var startNodeIdx = ToIdx(startCell);
        _openList.Insert(startNodeIdx, 0);

        Span<GridCoord> neighbours = stackalloc GridCoord[8];

        while(_openList.Size > 0)
        {
            var currentIdx = _openList.RemoveFirst();
            var currentCell = ToGridCoord(currentIdx);
            ref var current = ref _nodes[currentCell.Column, currentCell.Row];

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

            if (currentCell != startCell && current.ParentIdx != startNodeIdx)
            {
                var parentCell = ToGridCoord(current.ParentIdx);
                var parent = _nodes[parentCell.Column, parentCell.Row];
                var grandParentCell = ToGridCoord(parent.ParentIdx);

                if (HasLineOfSight(parentCell, grandParentCell))
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
                if (_unwalkablesCells[neighbourCell.Column, neighbourCell.Row]) { continue; }

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

    /// <summary>
    /// Returns the coordinate of the cell containing the provided world position Vector 
    /// </summary>
    /// <param name="position"></param>
    /// <returns>The coordinate of the cell containing the provided world position Vector </returns>
    public GridCoord ToGridCoordinates(Vector2 position)
    {
        var scaled = new Vector2(position.X * InverseCellSize, -position.Y * InverseCellSize);
        return new(World0ToGrid.Column + (int)MathF.Floor(scaled.X), World0ToGrid.Row + (int)MathF.Floor(scaled.Y));
    }

    /// <summary>
    /// Returns The center of the cell as world position
    /// </summary>
    /// <param name="coordinates"></param>
    /// <returns>The center of the cell as world position</returns>
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



    public int GetDistance(GridCoord a, GridCoord b)
    {
        var dx = (int)MathF.Abs(b.Column - a.Column); 
        var dy = (int)MathF.Abs(b.Row - a.Row);
        
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

    public GridCoord ToGridCoord(Idx<PathFindingNode> idx)
    {
        var rowCount = _nodes.GetLength(1);
        return new GridCoord(idx.Value / rowCount, idx.Value % rowCount);
    }

    public Idx<PathFindingNode> ToIdx(GridCoord coordinates)
    {
        return (Idx<PathFindingNode>)(coordinates.Column * _nodes.GetLength(1) + coordinates.Row);
    }

    public GridCoord FindNearestWalkableCell(GridCoord destination)
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
                var neighbour = current + new GridCoord(x, y);
                if (_nodes[neighbour.Column, neighbour.Row].State == PathFindingNode.NodeState.NearestWalkableVisited ||
                    neighbour.Column < 0 || neighbour.Column >= _unwalkablesCells.GetLength(0) ||
                    neighbour.Row < 0 || neighbour.Row >= _unwalkablesCells.GetLength(1))
                {
                    continue;
                }

                if (!_unwalkablesCells[neighbour.Column, neighbour.Row]) { return neighbour; }

                _nearestWalkableQueue[queueEnd++] = neighbour;
                _nodes[neighbour.Column, neighbour.Row].State = PathFindingNode.NodeState.NearestWalkableVisited;
            }
        }

        return destination;
    }
}