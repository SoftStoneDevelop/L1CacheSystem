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
public class ForeachByColumnBench
{
    private FakeMeterFactory? _meterFactory;
    private CacheSystem? _cacheSystem;

    [Params(10_000, 25_000, 50_000, 100_000, 1_000_000)]
    public int Size;

    [IterationSetup]
    public void IterationSetup()
    {
        _meterFactory = new FakeMeterFactory();
        _cacheSystem = new CacheSystem(NullLogger.Instance, TimeProvider.System, _meterFactory);
        _cacheSystem.StartAsync(default).Wait();

        for (int i = 0; i < Size; i++)
        {
            var haveOptional = i % 2 == 1;
            var value = new ValueClass()
            {
                Child = haveOptional ? new ValueClass()
                {
                    Id = i * 5,
                    IntProp = i * 3,
                    DoubleProp = i * 3d,
                    Child = null
                } : null,
                Id = i,
                IntProp = i * 2,
                DoubleProp = haveOptional ? i * 3d : null,
            };
            _cacheSystem.SetBitsNone(value, ValueOwnership.Transferred);
            _cacheSystem!.SetCustomFromView(
                value.Id,
                new CacheSystem.SetViewInfo
                {
                    IdProp = CacheSystem.SetViewOperation.Set,
                    FuncIdProp = () => value.Id,
                    IntProp = CacheSystem.SetViewOperation.Set,
                    FuncIntProp = () => value.IntProp,
                    DoubleProp = value.DoubleProp.HasValue ? CacheSystem.SetViewOperation.Set : CacheSystem.SetViewOperation.RemoveIfExist,
                    FuncDoubleProp = value.DoubleProp.HasValue ? () => value.DoubleProp : null,
                    ChildProp = value.Child != null ? CacheSystem.SetViewOperation.Set : CacheSystem.SetViewOperation.RemoveIfExist,
                    FuncChildProp = value.Child != null ? () => value.Child : null
                });
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _cacheSystem.StopAsync(default).Wait();
        _cacheSystem = null;
        _meterFactory = null;
    }

    [Benchmark(Description = "Entity", Baseline = true)]
    public async Task GetEntity()
    {
        var sum = 0;
        _cacheSystem.ForeachBitsNone((key, value) => { sum += value.IntProp; });
    }

    [Benchmark(Description = "Column")]
    public async Task GetColumn()
    {
        var sum = 0;
        _cacheSystem.ForeachByIntPropColumn((key, intProp) => { sum += intProp; });
    }
}