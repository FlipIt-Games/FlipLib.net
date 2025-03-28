using System.Numerics;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace FlipLib.AI;

public struct WayPoint
{
    public Vector2 Position;
    public Idx<WayPoint>? Next;
}

public struct Path
{
    public Idx<WayPoint>? FirstIdx;
    public Idx<WayPoint>? LastIdx;

    public Memory<WayPoint> _nodes;
    public BitVector32 _availableSpaces;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Path(int size, IAllocator allocator = null)
    {
        if (size > 32)
        {
            throw new ArgumentOutOfRangeException(nameof(size), "memory capacity must be less than or equal to 32");
        }

        _availableSpaces =  new(int.MaxValue);
        _nodes = allocator is null
            ? new WayPoint[size]
            : allocator.AllocZeroed<WayPoint>(size);
    }

    public ref WayPoint this[Idx<WayPoint> idx]
    {
        get 
        {
#if DEBUG
            if (_availableSpaces[1 << idx.Value])
            {
                throw new ArgumentException($"node at {idx.Value} is empty");
            }
#endif
            return ref _nodes.Span[idx.Value];
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsEmpty()
        => _availableSpaces.Data == int.MaxValue;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WayPoint? GetFirst() 
        => FirstIdx.HasValue ? _nodes.Span[FirstIdx.Value.Value] : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public WayPoint? GetLast() 
        => LastIdx.HasValue ? _nodes.Span[LastIdx.Value.Value] : null;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Idx<WayPoint> Push(Vector2 position)
    {
        var idx = Add(new WayPoint { Position = position });
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

    public Idx<WayPoint> PushFront(Vector2 position)
    {
        var idx = Add(new WayPoint { Position = position});
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
    private Idx<WayPoint> Add(WayPoint node)
    {
        for (int i = 0; i < 32; i++)
        {
            var mask = 1 << i;
            if (_availableSpaces[mask]) 
            {
                _nodes.Span[i] = node;
                _availableSpaces[mask] = false;
                return new Idx<WayPoint>(i);
            }
        }

        Console.WriteLine($"availableSpace = {_availableSpaces.Data:b}");
        throw new OutOfMemoryException("There is no more room in path for additional nodes");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Idx<WayPoint> Add()
    {
        for (int i = 0; i < 32; i++)
        {
            var mask = 1 << i;
            if (_availableSpaces[mask]) 
            {
                _availableSpaces[mask] = false;
                return new Idx<WayPoint>(i);
            }
        }

        throw new OutOfMemoryException("There is no more room in path for additional nodes");
    }
}
