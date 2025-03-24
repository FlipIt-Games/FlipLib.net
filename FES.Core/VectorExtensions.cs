using System.Numerics;

public static class VectorExtensions
{
    public static Vector2 Round(this Vector2 vector, MidpointRounding midpointRounding = MidpointRounding.ToEven)
        => new Vector2(
            (float)Math.Round(vector.X, midpointRounding),
            (float)Math.Round(vector.Y, midpointRounding)
        );
}
