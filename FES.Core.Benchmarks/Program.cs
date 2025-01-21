using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace FES.Benchmarks;

public class Program 
{
    public static void Main()
    {
        BenchmarkRunner.Run<PoolBenchmarks>();
    }
}

public class PoolBenchmarks
{
    private int[] raw10;
    private int[] raw1000;
    private int[] raw1_000_000;

    private Pool<int> pool10;
    private Pool<int> pool1000;
    private Pool<int> pool1_000_000;

    public int[] results;

    public PoolBenchmarks()
    {
        raw10 = InitializeArray(10);
        raw1000 = InitializeArray(1000);
        raw1_000_000 = InitializeArray(1_000_000);

        pool10 = InitializePool(10);
        pool1000 = InitializePool(1000);
        pool1_000_000 = InitializePool(1_000_000);

        results = new int[6];
    }

    public int[] InitializeArray(int size)
    {
        var rnd = new Random();
        var res = new int[size];

        for (int i = 0; i < size; i++) 
        {
            res[i] = rnd.Next(0, 10);
        }
        return res;
    }

    public Pool<int> InitializePool(int size)
    {
        var rnd = new Random();
        var pool = new Pool<int>(size);

        for (int i = 0; i < size; i++) 
        {
            pool.Add(rnd.Next(0, 10));
        }

        return pool;
    }

    [Benchmark]
    public int IteratingRaw10()
    {
        var sum = 0;
        for (int i = 0; i < raw10.Length; i++) 
        {
            sum += raw10[i];
        }

        return sum;
    }

    [Benchmark]
    public int IteratingRaw1000()
    {
        var sum = 0;
        for (int i = 0; i < raw1000.Length; i++) 
        {
            sum += raw1000[i];
        }

        return sum;
    }

    [Benchmark]
    public int IteratingRaw1_000_000()
    {
        var sum = 0;
        for (int i = 0; i < raw1_000_000.Length; i++) 
        {
            sum += raw1_000_000[i];
        }

        return sum;
    }

    [Benchmark]
    public int IteratingPool10()
    {
        var sum = 0;

        foreach(var i in pool10) 
        {
            sum += i;
        }

        return sum;
    }

    [Benchmark]
    public int IteratingPool1000()
    {
        var sum = 0;
        foreach(var i in pool1000) 
        {
            sum += i;
        }

        return sum;
    }

    [Benchmark]
    public int IteratingPool1_000_000()
    {
        var sum = 0;
        foreach(var i in pool1_000_000) 
        {
            sum += i;
        }
        return sum;
    }

    [Benchmark]
    public int IteratingForeach10()
    {
        var sum = 0;

        foreach(var i in raw10) 
        {
            sum += i;
        }

        return sum;
    }

    [Benchmark]
    public int IteratingForeach1000()
    {
        var sum = 0;
        foreach(var i in raw1000) 
        {
            sum += i;
        }

        return sum;
    }

    [Benchmark]
    public int IteratingForeach1_000_000()
    {
        var sum = 0;
        foreach(var i in raw1_000_000) 
        {
            sum += i;
        }
        return sum;
    }
}
