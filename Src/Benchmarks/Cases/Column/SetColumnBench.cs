using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.Backend;
using Benchmarks.Model;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

namespace Benchmarks.Cases.Column;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class SetColumnBench
{
    private FakeMeterFactory _meterFactory;
    private CacheSystem _cacheSystem;

    [Params(10_000, 25_000, 50_000, 100_000)]
    public int Size;

    [IterationSetup]
    public void IterationSetup()
    {
        _meterFactory = new FakeMeterFactory();
        _cacheSystem = new CacheSystem(NullLogger.Instance, TimeProvider.System, _meterFactory);
        _cacheSystem.StartAsync(default).Wait();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _cacheSystem.StopAsync(default).Wait();
        _cacheSystem = null;
        _meterFactory = null;
    }

    [Benchmark(Description = "Set Entity", Baseline = true)]
    public async Task GetEntityRef()
    {
        for (int i = 0; i < Size; i++)
        {
            _cacheSystem.SetBitsNone(new ValueClass()
            {
                Child = null,
                Id = i,
                IntProp = i * 2,
                DoubleProp = null
            },
            ValueOwnership.Transferred);
        }
    }

    [Benchmark(Description = "Set Column")]
    public async Task SetColumn()
    {
        for (int i = 0; i < Size; i++)
        {
            _cacheSystem.SetCustomId(i, i);
            _cacheSystem.SetCustomIntProp(i, i * 2);
        }
    }
}