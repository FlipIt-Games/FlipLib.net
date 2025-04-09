namespace FlipLib.Memory;

using System;
using System.Runtime.CompilerServices;

public sealed class Arena : IAllocator
{
    public byte[] _buffer;
    public int _offset;
    
    public Arena(int size)
    {
        _buffer = new byte[size];
        _offset = 0;
    }
    
    public Memory<T> AllocZeroed<T>(int count) where T : struct
    {
        var mem = AllocNonZeroed<T>(count);
        mem.Span.Clear(); 
        return mem;
    }

    public Memory<T> AllocNonZeroed<T>(int count) where T : struct
    {
        var alignment = Unsafe.SizeOf<T>();
        var alignedOffset = (_offset + (alignment - 1)) & ~(alignment - 1);
        
        _offset = alignedOffset + alignment * count;
        return new Memory<T>(Unsafe.As<byte[], T[]>(ref _buffer), alignedOffset / alignment, count);
    }

    public void FreeAll()
    {
        _offset = 0;
    }
}
