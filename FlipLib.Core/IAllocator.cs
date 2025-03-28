namespace FlipLib;

public interface IAllocator
{
    Memory<T> AllocZeroed<T>(int count) where T : struct;

    Memory<T> AllocNonZeroed<T>(int count) where T : struct;

    void FreeAll();
}
