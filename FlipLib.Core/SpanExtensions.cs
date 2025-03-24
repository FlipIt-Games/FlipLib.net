namespace FES;

public static class SpanExtensions
{
    public static void Insert<T>(this Span<T> span, T elem, int idx) 
    {
        for (int i = span.Length -2; i > idx; i--)   
        {
            ref var value = ref span[i];
            value = span[i -1];
        }

        ref var val = ref span[idx];
        val = elem;
    }
}