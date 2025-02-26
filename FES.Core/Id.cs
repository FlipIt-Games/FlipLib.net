using System.Diagnostics.CodeAnalysis;

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

    public static bool operator ==(Id<T> id, Id<T>other) => id.Value == other.Value;
    public static bool operator !=(Id<T> id, Id<T>other) => id.Value != other.Value;

    public override bool Equals([NotNullWhen(true)] object obj) => Value.Equals(obj);
    public override int GetHashCode() => Value.GetHashCode();

    public void Test()
    {
        Console.WriteLine("Test");
    }
}
