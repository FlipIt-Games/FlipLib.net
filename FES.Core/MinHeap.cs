namespace FES;

public struct MinHeapNode<T> where T : struct
{
    public int Value;
    public Idx<T> DataIdx;
}

public struct MinHeap<T> where T : struct
{
    public readonly int Capacity;
    public int Size;

    public MinHeapNode<T>[] _data;

    public MinHeap(int capacity)
    {
        Capacity = capacity;
        Size = 0;

        _data = new MinHeapNode<T>[capacity];
    }

    public MinHeapNode<T> this[Idx<MinHeapNode<T>> idx] 
    {
        get => _data[idx.Value];
        set => _data[idx.Value] = value;
    }

    public Idx<MinHeapNode<T>> GetLeftChildIdx(Idx<MinHeapNode<T>> idx)
        => (Idx<MinHeapNode<T>>)((2 * idx.Value) + 1);

    public Idx<MinHeapNode<T>> GetRightChildIdx(Idx<MinHeapNode<T>> idx)
        => (Idx<MinHeapNode<T>>)((2 * idx.Value) + 2);

    public Idx<MinHeapNode<T>> GetParentIdx(Idx<MinHeapNode<T>> idx)
        => (Idx<MinHeapNode<T>>)((idx.Value -1) * 0.5f);

    public void Insert(Idx<T> dataIdx, int value) 
    {
#if DEBUG
        if (Size >= Capacity)
        {
            throw new OutOfMemoryException("Heap is full");
        }
#endif
        var node = new MinHeapNode<T> { Value = value, DataIdx = dataIdx };
        var idx = new Idx<MinHeapNode<T>>(Size++);
        this[idx] = node;

        BubbleUp(idx);
    }

    // Refactor to not be O(n)
    public void Update(Idx<T> dataIdx, int newValue)
    {
        int i;
        for (i = 0; i < _data.Length; i++)
        {
            if (_data[i].DataIdx == dataIdx) 
            {
                break;
            }
        }

        var idx = (Idx<MinHeapNode<T>>)i;
        var item = this[idx];
        this[idx] = new() { Value = newValue, DataIdx = item.DataIdx };

        if (newValue < item.Value) { BubbleUp(idx); }
        else { BubbleDown(idx); }
    }

    public void RemoveFirst()
    {
        throw new NotImplementedException();
    }

    private void BubbleUp(Idx<MinHeapNode<T>> idx)
    {
        var currentIdx = idx;
        var parentIdx = GetParentIdx(idx);
        while(currentIdx.Value > 0 && this[parentIdx].Value > this[currentIdx].Value)
        {
            var tmp = this[currentIdx];
            this[currentIdx] = this[parentIdx];
            this[parentIdx] = tmp;

            currentIdx = GetParentIdx(currentIdx);
        }
    }

    private void BubbleDown(Idx<MinHeapNode<T>> idx)
    {
        var currentIdx = idx;    
        var leftChildIdx = GetLeftChildIdx(idx);
        var rightChildIdx = GetRightChildIdx(idx);

        while(leftChildIdx.Value < Size)
        {
            var current = this[currentIdx];
            var leftChild = this[leftChildIdx];

            var swapIndex = leftChildIdx;
            var swapNode = leftChild;

            if (rightChildIdx.Value < Size)
            {
                var rightChild = this[currentIdx];
                if (rightChild.Value < leftChild.Value)
                {
                    swapIndex = rightChildIdx;
                    swapNode = rightChild;
                }
            }

            if (current.Value <= swapNode.Value) { return; }

            var tmp = current;
            this[currentIdx] = swapNode;
            this[swapIndex] = current;

            currentIdx = swapIndex;
        }
    }
}