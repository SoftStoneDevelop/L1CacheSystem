using BenchmarkDotNet.Running;

namespace Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<GetMemoryCacheBench>();
        BenchmarkRunner.Run<SetMemoryCacheBench>();
        BenchmarkRunner.Run<UpdateMemoryCacheBench>();
        //BenchmarkRunner.Run<MetricEffectMemoryCacheBench>();
    }
}