namespace FlipLib;

public static class FloatExtensions
{
    public static bool Approximately(this float f1, float f2, float precision = float.Epsilon)
        => MathF.Abs(f2 - f1) <= precision;
}
