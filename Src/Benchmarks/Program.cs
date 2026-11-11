using BenchmarkDotNet.Running;
using Benchmarks.Cases.Column;

namespace Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<GetColumnBench>();
        BenchmarkRunner.Run<SetColumnBench>();
        BenchmarkRunner.Run<SetViewBench>();
        BenchmarkRunner.Run<ForeachByColumnBench>();

        //BenchmarkRunner.Run<GetMemoryCacheBench>();
        //BenchmarkRunner.Run<SetMemoryCacheBench>();
        //BenchmarkRunner.Run<UpdateMemoryCacheBench>();
        //BenchmarkRunner.Run<DeleteMemoryCacheBench>();
        //BenchmarkRunner.Run<MetricEffectMemoryCacheBench>();
    }
}