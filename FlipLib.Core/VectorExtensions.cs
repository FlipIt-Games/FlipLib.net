using System.Numerics;

namespace FlipLib;

public static class VectorExtensions
{
    public static Vector2 Round(this Vector2 vector, MidpointRounding midpointRounding = MidpointRounding.ToEven)
        => new Vector2(
            (float)Math.Round(vector.X, midpointRounding),
            (float)Math.Round(vector.Y, midpointRounding)
        );

    public static bool Approximately(this Vector2 v1, Vector2 v2, float precision = float.Epsilon)
        => v1.X.Approximately(v2.X, precision) || v1.Y.Approximately(v2.Y, precision);
}
