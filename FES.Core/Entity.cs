using System.Numerics;

namespace FES;

public readonly struct Id<T> where T : struct 
{
    public readonly int Value;

    public Id(int value) 
    {
        Value = value;
    }

    public static explicit operator int (Id<T> id) => id.Value;
    public static explicit operator Id<T>(int id) => new Id<T>(id);
}

public struct Entity
{
    public Vector2 Position;
    public float Rotation;
}
