using System.Runtime.CompilerServices;

namespace FES;

/// <summary>
/// A non resizable, contiguous and pre-allocated block of memory that allows for quick iteration, insertion and deletion.
/// Elements are unordered.
/// </summary>
/// <typeparam name="TEntityType"></typeparam>
public struct Pool<TEntityType> 
    where TEntityType : struct 
{
    private TEntityType[] datas;
    private int capacity;
    private int size;

    /// <summary>
    /// The max number of elements the pool can contain.
    /// </summary>
    public int Capacity => capacity; 

    /// <summary>
    /// The current number of elements in the pool.
    /// </summary>
    public int Size => size;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pool(int capacity) 
    {
        size = 0;
        datas = new TEntityType[capacity];
        this.capacity = capacity;
    }

    /// <summary>
    /// Trying to access elements over the size of the pool results in IndexOutOfRangeException in Debug Mode.
    /// </summary>
    /// <param name="idx"></param>
    /// <returns>The element at idx idx</returns>
    public ref TEntityType this[Id<TEntityType> idx]
    {
        get 
        {
#if DEBUG
            if ((int)idx >= Size) 
            {
                throw new IndexOutOfRangeException(nameof(idx));
            }
#endif
            return ref datas[(int)idx];
        }
    }

    /// <summary>
    /// Returns a ref to the first available element and increments the size of the pool. 
    /// Datas are not reinitialized and might contains the values of the previous object. 
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TEntityType Add() 
    {
#if DEBUG
        if (size == capacity) 
        {
            throw new OutOfMemoryException();
        }
#endif
        return ref datas[size++];
    }

    /// <summary>
    /// Push the values of item onto the first available slot and returns a ref to it while incrementing the size of the pool.
    /// </summary>
    /// <param name="item"></param>
    /// <returns>A ref to the inserted element.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TEntityType Add(TEntityType item) 
    {
#if DEBUG
        if (size == capacity) 
        {
            throw new OutOfMemoryException();
        }
#endif

        datas[size++] = item;
        return ref datas[size -1];
    }

    /// <summary>
    /// Removes the datas at id from the pool and decrement the pool size.
    /// Datas at id are swapped with the datas at the last active element index.
    /// /!\ Does not preserve ordering /!\.
    /// </summary>
    /// <param name="id"></param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(Id<TEntityType> id)
    {
#if DEBUG
        if ((int)id >= Size) 
        {
            throw new IndexOutOfRangeException(nameof(id));
        }
#endif
        var data = datas[(int)id];
        var swapWith = datas[--size];

        datas[size] = data;
        datas[(int)id] = swapWith;
    }

    /// <summary>
    /// Resets the size to 0. Effectively marking the pool as empty.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() 
    {
        size = 0;
    } 
}
