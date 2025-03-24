using System.Numerics;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;

namespace FES.AI;

public struct Path
{
    public Idx<PathNode>? FirstIdx;
    public Idx<PathNode>? LastIdx;

    public PathNode[] _nodes;
    public BitVector32 _availableSpaces;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Path(int capacity)
    {
        if (capacity > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "capacity must be less than or equal to 32");
        }

        _nodes = new PathNode[capacity];
        _availableSpaces =  new(int.MaxValue);
    }

    public ref PathNode this[Idx<PathNode> idx]
    {
        get 
        {
#if DEBUG
            if (_availableSpaces[1 << idx.Value])
            {
                throw new ArgumentException($"node at {idx.Value} is empty");
            }
#endif
            return ref _nodes[idx.Value];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty()
        => _availableSpaces.Data == int.MaxValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PathNode? GetFirst() 
        => FirstIdx.HasValue ? _nodes[FirstIdx.Value.Value] : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public PathNode? GetLast() 
        => LastIdx.HasValue ? _nodes[LastIdx.Value.Value] : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Idx<PathNode> Push(Vector2 position)
    {
        var idx = Add(new PathNode { Position = position });
        if (!LastIdx.HasValue)
        {
            FirstIdx = LastIdx = idx;
            return idx;
        }

        ref var last = ref this[LastIdx.Value];
#if DEBUG
        if (last.Position == position)
        {
            Console.WriteLine("WARNING: Trying to push the same position to the path");
            var node = GetFirst()!.Value;
            var count = 1;
            Console.WriteLine("Printing path");
            while(true)
            {
                Console.WriteLine($"{count}: {node.Position}");
                count++;
                if (node.Next.HasValue) { node = this[node.Next.Value]; }
                else { break; }
            }
            Console.WriteLine("Printing path end");
        }
#endif

        last.Next = idx;
        LastIdx = idx;
        return idx;
    }

    public Idx<PathNode> PushFront(Vector2 position)
    {
        var idx = Add(new PathNode { Position = position});
        if (!FirstIdx.HasValue)
        {
            FirstIdx = LastIdx = idx; 
            return idx;
        }

        ref var first = ref this[idx];
        first.Next = FirstIdx.Value; 
        FirstIdx = idx;

        return idx; 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveFirst()
    {
        if (!FirstIdx.HasValue) { return; }
#if DEBUG
        var firstIdx = FirstIdx.Value;
        FirstIdx = this[firstIdx].Next;
        _availableSpaces[1 << firstIdx.Value] = true;
#else
        _availableSpaces[1 << FirstIdx.Value.Value] = true;
        FirstIdx = this[FirstIdx.Value].Next;
#endif
        if (!FirstIdx.HasValue) { Clear(); }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _availableSpaces = new(int.MaxValue);
        FirstIdx = LastIdx = null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Idx<PathNode> Add(PathNode node)
    {
        for (int i = 0; i < 32; i++)
        {
            var mask = 1 << i;
            if (_availableSpaces[mask]) 
            {
                _nodes[i] = node;
                _availableSpaces[mask] = false;
                return new Idx<PathNode>(i);
            }
        }

        Console.WriteLine($"availableSpace = {_availableSpaces.Data:b}");
        throw new OutOfMemoryException("There is no more room in path for additional nodes");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Idx<PathNode> Add()
    {
        for (int i = 0; i < 32; i++)
        {
            var mask = 1 << i;
            if (_availableSpaces[mask]) 
            {
                _availableSpaces[mask] = false;
                return new Idx<PathNode>(i);
            }
        }

        throw new OutOfMemoryException("There is no more room in path for additional nodes");
    }
}

public struct PathNode
{
    public Vector2 Position;
    public Idx<PathNode>? Next;
}