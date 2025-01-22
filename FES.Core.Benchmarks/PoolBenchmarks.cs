using BenchmarkDotNet.Attributes;

namespace FES.Core.Benchmarks;

public struct DumbStruct
{
    public int A;
    public int B;
    public float C;
    public float D;
    public double E;

    public DumbStruct(Random rnd) 
    {
        A = rnd.Next(0, 10);
        B = rnd.Next(0, 10);
        C = rnd.Next(0, 10);
        D = rnd.Next(0, 10);
        E = rnd.Next(0, 10);
    }
}

[MemoryDiagnoser(true)]
public class PoolBenchmarks
{
    [Params(10, 100, 1000, 10_000)]
    public int Length;

    private DumbStruct[] arr;
    private Pool<DumbStruct> pool;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random();
        arr = new DumbStruct[Length];
        pool = new Pool<DumbStruct>(Length);

        for (int i = 0; i < Length; i++) 
        {
            arr[i] = new DumbStruct(rnd);
            pool.Add(new DumbStruct(rnd));
        }
    } 

    [Benchmark]
    public double ArrSumForLoopRefRO()
    {
        double sum = 0;
        for (int i = 0; i < arr.Length; i++) 
        {
            ref readonly var data = ref arr[i];
            sum += data.A + data.B + data.C + data.D + data.E;
        }

        return sum;
    }

    [Benchmark]
    public double ArrSumForeach()
    {
        
        double sum = 0;
        foreach (var data in arr)
        {
            sum += data.A + data.B + data.C + data.D + data.E;
        }

        return sum;
    }

    [Benchmark]
    public double PoolSumRefRO()
    {
        
        double sum = 0;
        foreach (ref readonly var data in pool)
        {
            sum += data.A + data.B + data.C + data.D + data.E;
        }

        return sum;
    }
}
