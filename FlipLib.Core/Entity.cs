namespace FlipLib;

public struct Entity<T> where T : struct
{
    public UId<T> Id;
    public T Item;
}

public readonly struct Idx<T> where T : struct 
{
    public readonly int Value;

    public Idx(int value) 
    {
        Value = value;
    }

    public static explicit operator int (Idx<T> id) => id.Value;
    public static explicit operator Idx<T>(int id) => new Idx<T>(id);

    public static bool operator ==(Idx<T> id, Idx<T>other) => id.Value == other.Value;
    public static bool operator !=(Idx<T> id, Idx<T>other) => id.Value != other.Value;

    public override bool Equals(object obj) => Value.Equals(obj);
    public override int GetHashCode() => Value.GetHashCode();
}

public readonly struct UId<T> where T : struct
{
    public readonly int Value;

    public UId(int value)
    {
        Value = value;
    }

    public static explicit operator int (UId<T> id) => id.Value;
    public static explicit operator UId<T>(int id) => new UId<T>(id);
    public static bool operator ==(UId<T> id, UId<T>other) => id.Value == other.Value;
    public static bool operator !=(UId<T> id, UId<T>other) => id.Value != other.Value;

    public override bool Equals(object obj) => Value.Equals(obj);
    public override int GetHashCode() => Value.GetHashCode();
}
