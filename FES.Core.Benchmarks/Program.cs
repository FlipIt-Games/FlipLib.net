using BenchmarkDotNet.Running;

namespace FES.Core.Benchmarks;

public class Program 
{
    public static void Main()
    {
        BenchmarkRunner.Run<PoolBenchmarks>();
    }
}
