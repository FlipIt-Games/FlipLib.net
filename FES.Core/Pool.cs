using System.Collections;
using System.Runtime.CompilerServices;

namespace FES;

public class Pool<TEntityType> 
    where TEntityType : struct 
{
    private TEntityType[] datas;
    public int Capacity { get; private set; }
    public int Size { get; private set; }

    private int currentEnumeratorIdx;


    public TEntityType Current => datas[currentEnumeratorIdx];

    private Pool() {}

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Pool(int capacity) 
    {
        datas = new TEntityType[capacity];
        Size = 0;
        this.Capacity = capacity;
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
        var swapWith = datas[Size -1];

        datas[Size -1] = swapWith;
        datas[(int)id] = data;

        if (currentEnumeratorIdx > 0) 
        {
            currentEnumeratorIdx -=1;
        }

        Size -= 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(TEntityType item) 
    {
#if DEBUG
        if (Size == Capacity) 
        {
            throw new OutOfMemoryException();
        }
#endif

        datas[Size] = item;
        Size += 1;
    }

    public Pool<TEntityType> GetEnumerator() 
    {
        currentEnumeratorIdx = -1;
        return this;
    }

    public bool MoveNext() 
    {
        return ++currentEnumeratorIdx != Size;

        // if (currentEnumeratorIdx >= Size) 
        // {
        //     return false;
        // }

        // current = datas[currentEnumeratorIdx];
        // currentEnumeratorIdx +=1;
        // return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() => currentEnumeratorIdx = 0;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose() {}
}
