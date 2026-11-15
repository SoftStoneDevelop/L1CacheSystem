using BenchmarkDotNet.Running;
using Benchmarks.Cases.Column;

namespace Benchmarks;

internal class Program
{
    static void Main()
    {
        BenchmarkRunner.Run<GetIntColumnBench>();
        BenchmarkRunner.Run<SetViewBench>();
        BenchmarkRunner.Run<GetViewBench>();
        BenchmarkRunner.Run<ViewInContextBench>();
        BenchmarkRunner.Run<UpdateColumnBench>();
        BenchmarkRunner.Run<OptionalBench>();

        //BenchmarkRunner.Run<GetMemoryCacheBench>();
        //BenchmarkRunner.Run<SetMemoryCacheBench>();
        //BenchmarkRunner.Run<UpdateMemoryCacheBench>();
        //BenchmarkRunner.Run<DeleteMemoryCacheBench>();
        //BenchmarkRunner.Run<MetricEffectMemoryCacheBench>();
    }
}