using System.Numerics;

namespace FlipLib.AI;

public struct LazyThetaNode 
{
    public enum NodeState
    {
        Unvisited,
        Open,
        Closed,
    }

    public int Generation;
    public NodeState State;

    public Idx<LazyThetaNode> ParentIdx;

    public int GValue;
    public int HValue;

    public int FValue => GValue + HValue;
}

public struct LazyThetaPathFinder
{
    // Tracks the number of times the alogorithm ran 
    // used to avoid clearing the nodes between runs
    public int _generation;

    public NavigationGrid _grid;
    public Memory<LazyThetaNode> _nodes;
    public MinHeap<LazyThetaNode> _openList;

    public LazyThetaPathFinder(float cellSize, int columns, int rows, IAllocator allocator = null)
    {
        _grid = new NavigationGrid(cellSize, columns, rows, allocator);
        var gridSize = _grid.ColumnCount * _grid.RowCount;
        
        _nodes = allocator is null
            ? new LazyThetaNode[gridSize]
            : allocator.AllocZeroed<LazyThetaNode>(gridSize);

        _openList = new MinHeap<LazyThetaNode>(gridSize, allocator);
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
        _generation++;
        path.Clear();
        _openList.Clear();

        // Set destination cell and correct it if it is overlapping with an unwalkable cell
        var startCell = _grid.ToGridCoordinates(origin);
        var endCell = _grid.ToGridCoordinates(destination);

        var startIdx = _grid.To1DIdx(startCell);
        var endIdx = _grid.To1DIdx(endCell);

        if ((_grid[endCell.Column, endCell.Row] & mask) != 0)
        {
            endCell = _grid.FindNearestWalkableCell(endCell, mask); 
        }

        var nodes = _nodes.Span;

        ref var startNode = ref nodes[startIdx];
        startNode.State = LazyThetaNode.NodeState.Open;
        startNode.Generation = _generation;

        var startNodeIdx = ToIdx(startCell);
        _openList.Insert(startNodeIdx, 0);

        Span<GridCoordinate> neighbours = stackalloc GridCoordinate[8];

        while(_openList.Size > 0)
        {
            var currentIdx = _openList.RemoveFirst();
            var currentCell = ToGridCoord(currentIdx);
            ref var current = ref nodes[currentIdx.Value];

            // If we reached the destination we can build the path by walking parents
            if (currentIdx.Value == endIdx) 
            {
                while(currentCell != startCell)
                {
                    path.PushFront(_grid.ToWorldPosition(currentCell));

                    currentCell = ToGridCoord(current.ParentIdx);
                    currentIdx = ToIdx(currentCell); 
                    current = nodes[currentIdx.Value];
                }
                return;
            }

            // Performs Line of Sight check to remove redundant nodes, this allows for "any angle" path creation
            // instead of grid constrained path
            if (currentCell != startCell && current.ParentIdx != startNodeIdx)
            {
                var parent = nodes[current.ParentIdx.Value];
                var grandParentCell = ToGridCoord(parent.ParentIdx);

                if (_grid.HasLineOfSight(currentCell, grandParentCell, mask))
                {
                    var grandParent = nodes[parent.ParentIdx.Value];
                    current.ParentIdx = parent.ParentIdx;
                    current.GValue = grandParent.GValue + _grid.GetDistance(grandParentCell, currentCell);
                }
            }

            current.State = LazyThetaNode.NodeState.Closed;
            current.Generation = _generation;

            var neighboursCount = _grid.GetNeighbours(neighbours, currentCell);
            for (int i = 0; i < neighboursCount; i++)
            {
                var neighbourCell = neighbours[i];
                var neighbourIdx = _grid.To1DIdx(neighbourCell);

                if ((_grid[neighbourCell] & mask) != 0) { continue; }

                var neighbour = nodes[neighbourIdx];
                if (neighbour.Generation == _generation && neighbour.State == LazyThetaNode.NodeState.Closed) 
                { 
                    continue; 
                }

                int cost = current.GValue + _grid.GetDistance(neighbourCell, currentCell);
                if (neighbour.Generation == _generation && neighbour.State == LazyThetaNode.NodeState.Open && cost >= neighbour.GValue) 
                {
                   continue; 
                }

                neighbour.GValue = cost;
                neighbour.HValue = _grid.GetDistance(neighbourCell, endCell);
                neighbour.ParentIdx = currentIdx;

                var idx = ToIdx(neighbourCell);
                if (neighbour.Generation == _generation && neighbour.State == LazyThetaNode.NodeState.Open)
                {
                    _openList.Update(idx, neighbour.FValue);
                }
                else 
                {
                    _openList.Insert(idx, neighbour.FValue);
                    neighbour.State = LazyThetaNode.NodeState.Open;
                    neighbour.Generation = _generation;
                }

                nodes[neighbourIdx] = neighbour;
            }
        }
    }

    public GridCoordinate ToGridCoord(Idx<LazyThetaNode> idx)
    {
        return _grid.ToGridCoordinate(idx.Value);
    }

    public Idx<LazyThetaNode> ToIdx(GridCoordinate coordinates)
    {
        return (Idx<LazyThetaNode>)_grid.To1DIdx(coordinates);
    }
}
