using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.Backend;
using Benchmarks.Model;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Benchmarks.Cases.Column;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[SimpleJob(RuntimeMoniker.Net11_0)]
[SimpleJob(RuntimeMoniker.NativeAot11_0)]
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

    [Benchmark(Description = "SetEntity", Baseline = true)]
    public void SetEntity()
    {
        var valuesSpan = _values!.AsSpan();
        for (int i = 0; i < valuesSpan.Length; i++)
        {
            var value = valuesSpan[i];
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

    [Benchmark(Description = "SetFromView")]
    public void SetFromView()
    {
        var view = new CacheSystem.SetViewColumnCache<ValueClass>
        {
            Id = SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            IntProp = SetViewOperation.Set,
            FuncIntProp = static (source) => source.IntProp,
            FuncDoubleProp = static (source) => source.DoubleProp.HasValue ? source.DoubleProp : null,
            FuncChild = static (source) => source.Child ?? null!
        };

        var valuesSpan = _values!.AsSpan();
        for (int i = 0; i < valuesSpan.Length; i++)
        {
            var value = valuesSpan[i];

            view.Source = value;
            view.DoubleProp = value.DoubleProp.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.Child = value.Child != null ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;

            _cacheSystem!.SetColumnCacheFromView(value.Id, in view);
        }
    }

    [Benchmark(Description = "WriteInContextInternal")]
    public void WriteInContextInternal()
    {
        _cacheSystem!.WriteInContextColumnCache(static (in context, state) =>
        {
            var valuesSpan = ((ValueClass[])state).AsSpan();

            for (int i = 0; i < valuesSpan.Length; i++)
            {
                var value = valuesSpan[i];
                context.SetColumnCacheId(value.Id, value.Id);
                context.SetColumnCacheIntProp(value.Id, value.IntProp);

                if (value.DoubleProp.HasValue)
                {
                    context.SetColumnCacheDoubleProp(value.Id, value.DoubleProp);
                }
                else
                {
                    context.DeleteDoubleProp(value.Id);
                }

                if (value.Child != null)
                {
                    context.SetColumnCacheChild(value.Id, value.Child);
                }
                else
                {
                    context.DeleteChild(value.Id);
                }
            }
        },
        _values!);
    }

    [Benchmark(Description = "WriteInContextInternalFromView")]
    public void WriteInContextInternalFromView()
    {
        _cacheSystem!.WriteInContextColumnCache(static (in context, state) =>
        {
            var valuesSpan = ((ValueClass[])state).AsSpan();
            var view = new CacheSystem.SetViewColumnCache<ValueClass>
            {
                Id = SetViewOperation.Set,
                FuncId = static (source) => source.Id,
                IntProp = SetViewOperation.Set,
                FuncIntProp = static (source) => source.IntProp,
                FuncDoubleProp = static (source) => source.DoubleProp.HasValue ? source.DoubleProp : null,
                FuncChild = static (source) => source.Child ?? null!
            };

            for (int i = 0; i < valuesSpan.Length; i++)
            {
                var value = valuesSpan[i];

                view.Source = value;
                view.DoubleProp = value.DoubleProp.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.Child = value.Child != null ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;

                context.SetColumnCacheFromView(value.Id, in view);
            }
        },
        _values!);
    }
}