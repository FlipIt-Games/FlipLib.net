using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace FES;


public class Pool<TEntityType> 
    where TEntityType : struct 
{
    private TEntityType[] datas;
    private int capacity;
    private int size;

    public int Capacity => capacity; 
    public int Size => size;

    private int currentEnumeratorIdx;

    public unsafe ref TEntityType Current 
    {
        get 
        {
            return ref datas[currentEnumeratorIdx];
        }
    }

    // [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pool(int capacity) 
    {
        size = 0;
        datas = new TEntityType[capacity];
        this.capacity = capacity;
        currentEnumeratorIdx = -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref TEntityType Get(Id<TEntityType> id)
    {
#if DEBUG
        if ((int)id >= Size) 
        {
            throw new IndexOutOfRangeException(nameof(id));
        }
#endif

        return ref datas[(int)id] ;
    }

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

        datas[Size] = swapWith;
        datas[(int)id] = data;

        currentEnumeratorIdx--; 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddStatic(ref Pool<TEntityType> pool, TEntityType item) 
    {
#if DEBUG
        if (pool.size == pool.capacity) 
        {
            throw new OutOfMemoryException();
        }
#endif

        pool.datas[pool.size++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TEntityType item) 
    {
#if DEBUG
        if (size == capacity) 
        {
            throw new OutOfMemoryException();
        }
#endif

        datas[size++] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pool<TEntityType> GetEnumerator() 
    {
        currentEnumeratorIdx = -1;
        return this;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool MoveNext() 
    {
        return ++currentEnumeratorIdx != this.Size;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() => currentEnumeratorIdx = 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {}
}