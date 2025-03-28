using System.Runtime.CompilerServices;

namespace FlipLib;

/// <summary>
/// A non resizable, contiguous and pre-allocated block of memory that allows for quick iteration, insertion and deletion.
/// Elements are unordered.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public struct Pool<TEntity> 
    where TEntity : struct 
{
    public Memory<Entity<TEntity>> _datas;
    public int _size;

    /// <summary>
    /// The max number of elements the pool can contain.
    /// </summary>
    public int Capacity => _datas.Length; 

    /// <summary>
    /// The current number of elements in the pool.
    /// </summary>
    public int Size => _size;

    private int nextUId;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pool(int capacity, IAllocator allocator = null) 
    {
        _size = 0;

        if (allocator is null)
        {
            _datas = new Entity<TEntity>[capacity];
        }
        else 
        {
            _datas = allocator.AllocZeroed<Entity<TEntity>>(capacity);
        }
    }

    /// <summary>
    /// Trying to access elements over the size of the pool results in IndexOutOfRangeException in Debug Mode.
    /// </summary>
    /// <param name="idx"></param>
    /// <returns>The element at idx idx</returns>
    public ref Entity<TEntity> this[Idx<TEntity> idx]
    {
        get 
        {
#if DEBUG
            if ((int)idx >= Size) 
            {
                throw new IndexOutOfRangeException(nameof(idx));
            }
#endif
            return ref _datas.Span[(int)idx];
        }
    }

    public TEntity? GetByUId(UId<TEntity> uid)
    {
        for (int i = 0; i < _size; i++)
        {
            ref var item = ref _datas.Span[i];
            if (item.Id == uid) { return item.Item; }
        }

        return null;
    }

    /// <summary>
    /// Returns a ref to the first available element and increments the size of the pool. 
    /// Datas are not reinitialized and might contains the values of the previous object. 
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity<TEntity> Add() 
    {
#if DEBUG
        if (_size == Capacity) 
        {
            throw new OutOfMemoryException();
        }
#endif
        ref var data = ref _datas.Span[_size++];
        data.Id = new UId<TEntity>(nextUId++);
        return ref data;
    }

    /// <summary>
    /// Push the values of item onto the first available slot and returns a ref to it while incrementing the size of the pool.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>A ref to the inserted element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref Entity<TEntity> Add(TEntity item) 
    {
#if DEBUG
        if (_size == Capacity) 
        {
            throw new OutOfMemoryException();
        }
#endif

        _datas.Span[_size++] = new Entity<TEntity> 
        { 
            Id = new UId<TEntity>(nextUId++),
            Item = item 
        };

        return ref _datas.Span[_size -1];
    }

    /// <summary>
    /// Removes the datas at id from the pool and decrement the pool size.
    /// Datas at id are swapped with the datas at the last active element index.
    /// /!\ Does not preserve ordering /!\.
    /// </summary>
    /// <param name="id"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Idx<TEntity> id)
    {
#if DEBUG
        if ((int)id >= Size) 
        {
            throw new IndexOutOfRangeException(nameof(id));
        }
#endif
        var data = _datas.Span[(int)id];
        var swapWith = _datas.Span[--_size];

        _datas.Span[_size] = data;
        _datas.Span[(int)id] = swapWith;
    }

    /// <summary>
    /// Resets the size to 0. Effectively marking the pool as empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() 
    {
        _size = 0;
    }

    /// <summary>
    /// </summary>
    /// <returns>Returns the underlying datas as span ranging from 0 to size</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ReadOnlySpan<Entity<TEntity>> AsSpan()
    {
        return _datas.Span.Slice(0, _size);
    }
}
