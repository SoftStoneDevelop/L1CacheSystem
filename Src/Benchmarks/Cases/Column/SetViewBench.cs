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
[SimpleJob(RuntimeMoniker.NativeAot10_0)]
public class SetViewBench
{
    private FakeMeterFactory? _meterFactory;
    private CacheSystem? _cacheSystem;
    private ValueClass[]? _values;

    [Params(10_000, 25_000, 50_000, 100_000)]
    public int Size;

    [IterationSetup]
    public void IterationSetup()
    {
        _meterFactory = new FakeMeterFactory();
        _cacheSystem = new CacheSystem(NullLogger.Instance, TimeProvider.System, _meterFactory);
        _cacheSystem.StartAsync(default).Wait();

        _values = new ValueClass[Size];
        for (int i = 0; i < Size; i++)
        {
            var haveOptional = i % 2 == 1;
            var value = new ValueClass()
            {
                Child = haveOptional ? new ValueClass()
                {
                    Id = i * 5,
                    IntProp = i * 3,
                    DoubleProp = i * 78451d,
                    Child = null
                } : null,
                Id = i,
                IntProp = i * 2,
                DoubleProp = haveOptional ? i * 8745d : null
            };

            _values[i] = value;
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _cacheSystem?.StopAsync(default).Wait();
        _cacheSystem = null;
        _meterFactory = null;
    }

    [Benchmark(Description = "Set Entity", Baseline = true)]
    public async Task GetEntityRef()
    {
        for (int i = 0; i < Size; i++)
        {
            var value = _values![i];
            var newVal = new ValueClass()
            {
                Id = value.Id,
                IntProp = value.IntProp,
                DoubleProp = value.DoubleProp,
                Child = value.Child
            };

            _cacheSystem!.SetBitsNone(newVal, ValueOwnership.Transferred);
        }
    }

    [Benchmark(Description = "Set ViewT")]
    public async Task SetTRefColumn()
    {
        var view = new CacheSystem.SetViewInfoRef<ValueClass>
        {
            IdProp = SetViewOperation.Set,
            FuncIdProp = static (source) => source.Id,
            IntProp = SetViewOperation.Set,
            FuncIntProp = static (source) => source.IntProp,
            FuncDoubleProp = static (source) => source.DoubleProp.HasValue ? source.DoubleProp : null,
            FuncChildProp = static (source) => source.Child ?? null
        };

        for (int i = 0; i < Size; i++)
        {
            var value = _values![i];

            view.Source = value;
            view.DoubleProp = value.DoubleProp.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.ChildProp = value.Child != null ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;

            _cacheSystem!.SetCustomFromView(value.Id, view);
        }
    }
}