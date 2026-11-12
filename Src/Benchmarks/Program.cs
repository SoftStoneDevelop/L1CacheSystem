using BenchmarkDotNet.Running;
using Benchmarks.Cases.Column;

namespace Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        //BenchmarkRunner.Run<ForeachByChildColumnBench>();
        //BenchmarkRunner.Run<GetIntColumnBench>();
        BenchmarkRunner.Run<SetViewBench>();
        //BenchmarkRunner.Run<ForeachByIntColumnBench>();
        //BenchmarkRunner.Run<UpdateColumnBench>();

        //BenchmarkRunner.Run<GetMemoryCacheBench>();
        //BenchmarkRunner.Run<SetMemoryCacheBench>();
        //BenchmarkRunner.Run<UpdateMemoryCacheBench>();
        //BenchmarkRunner.Run<DeleteMemoryCacheBench>();
        //BenchmarkRunner.Run<MetricEffectMemoryCacheBench>();
    }
}