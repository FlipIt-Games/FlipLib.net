using System.Runtime.CompilerServices;

namespace FES;

public static class ExceptionHelper 
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ThrowIfDebug(Exception exception) 
    {
#if DEBUG
        throw exception;
#endif
    }
}
