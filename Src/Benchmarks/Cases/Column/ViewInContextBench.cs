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
[SimpleJob(RuntimeMoniker.Net11_0)]
public class ViewInContextBench
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

        var view = new CacheSystem.SetViewColumnCache<ValueClass>
        {
            Id = SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            IntProp = SetViewOperation.Set,
            FuncIntProp = static (source) => source.IntProp,
            FuncDoubleProp = static (source) => source.DoubleProp.HasValue ? source.DoubleProp : null,
            FuncChild = static (source) => source.Child ?? null!
        };

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

            view.Source = value;
            view.DoubleProp = value.DoubleProp.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.Child = value.Child != null ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;

            _cacheSystem!.SetColumnCacheFromView(value.Id, view);
            _cacheSystem.SetBitsNone(value, ValueOwnership.Transferred);
        }
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _cacheSystem?.StopAsync(default).Wait();
        _cacheSystem = null;
        _meterFactory = null;
    }

    [Benchmark(Description = "Entity", Baseline = true)]
    public async Task ForeachEntity()
    {
        var sum = 0;
        _cacheSystem!.ForeachBitsNone((value) => { sum += value.IntProp; });
    }

    [Benchmark(Description = "ViewInContext")]
    public async Task SpanIn()
    {
        var sum = 0;
        _cacheSystem!.ViewInContextColumnCache((context) =>
        {
            var span = context.GetIntPropSpan();
            for (int i = 0; i < span.Length; i++)
            {
                sum += span[i];
            }
        });
    }
}