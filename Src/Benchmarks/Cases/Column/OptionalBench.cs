using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.Backend;
using Benchmarks.Model;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using System;

namespace Benchmarks.Cases.Column;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net11_0)]
[SimpleJob(RuntimeMoniker.NativeAot11_0)]
public class OptionalBench
{
    private FakeMeterFactory? _meterFactory;
    private CacheSystem? _cacheSystem;
    private ModelWithOptions[]? _values;

    [Params(10_000, 50_000, 100_000, 1_000_000)]
    public int Size;

    [IterationSetup]
    public void IterationSetup()
    {
        _meterFactory = new FakeMeterFactory();
        _cacheSystem = new CacheSystem(NullLogger.Instance, TimeProvider.System, _meterFactory);
        _cacheSystem.StartAsync(default).Wait();

        _values = new ModelWithOptions[Size];
        for (int i = 0; i < Size; i++)
        {
            var value = new ModelWithOptions()
            {
                Id = i,
                DoubleProp1 = i % 10 == 1 ? i * 8745d : null,
                DoubleProp2 = i % 10 == 2 ? i * 8745d : null,
                DoubleProp3 = i % 10 == 3 ? i * 8745d : null,
                DoubleProp4 = i % 10 == 4 ? i * 8745d : null,
                DoubleProp5 = i % 10 == 5 ? i * 8745d : null,
                DoubleProp6 = i % 10 == 6 ? i * 8745d : null,
                DoubleProp7 = i % 10 == 7 ? i * 8745d : null,
                DoubleProp8 = i % 10 == 8 ? i * 8745d : null,
                DoubleProp9 = i % 10 == 9 ? i * 8745d : null,
                DoubleProp10 = i % 10 == 0 ? i * 8745d : null,
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

    [Benchmark(Description = "Entity Set", Baseline = true)]
    public void SetEntity()
    {
        var valuesSpan = _values!.AsSpan();
        for (int i = 0; i < valuesSpan.Length; i++)
        {
            var value = valuesSpan[i];
            var newVal = new ModelWithOptions()
            {
                Id = value.Id,
                DoubleProp1 = value.DoubleProp1,
                DoubleProp2 = value.DoubleProp2,
                DoubleProp3 = value.DoubleProp3,
                DoubleProp4 = value.DoubleProp4,
                DoubleProp5 = value.DoubleProp5,
                DoubleProp6 = value.DoubleProp6,
                DoubleProp7 = value.DoubleProp7,
                DoubleProp8 = value.DoubleProp8,
                DoubleProp9 = value.DoubleProp9,
                DoubleProp10 = value.DoubleProp10,
            };

            _cacheSystem!.SetOptionalModel(newVal, ValueOwnership.Transferred);
        }
    }

    [Benchmark(Description = "Column SetFromView Naive")]
    public void SetFromView()
    {
        var view = new CacheSystem.SetViewColumnOptionalModel<ModelWithOptions>
        {
            Id = SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            FuncDoubleProp1 = static (source) => source.DoubleProp1.HasValue ? source.DoubleProp1 : null,
            FuncDoubleProp2 = static (source) => source.DoubleProp2.HasValue ? source.DoubleProp2 : null,
            FuncDoubleProp3 = static (source) => source.DoubleProp3.HasValue ? source.DoubleProp3 : null,
            FuncDoubleProp4 = static (source) => source.DoubleProp4.HasValue ? source.DoubleProp4 : null,
            FuncDoubleProp5 = static (source) => source.DoubleProp5.HasValue ? source.DoubleProp5 : null,
            FuncDoubleProp6 = static (source) => source.DoubleProp6.HasValue ? source.DoubleProp6 : null,
            FuncDoubleProp7 = static (source) => source.DoubleProp7.HasValue ? source.DoubleProp7 : null,
            FuncDoubleProp8 = static (source) => source.DoubleProp8.HasValue ? source.DoubleProp8 : null,
            FuncDoubleProp9 = static (source) => source.DoubleProp9.HasValue ? source.DoubleProp9 : null,
            FuncDoubleProp10 = static (source) => source.DoubleProp10.HasValue ? source.DoubleProp10 : null,
        };

        var valuesSpan = _values!.AsSpan();
        for (int i = 0; i < valuesSpan.Length; i++)
        {
            var value = valuesSpan[i];

            view.Source = value;
            view.DoubleProp1 = value.DoubleProp1.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp2 = value.DoubleProp2.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp3 = value.DoubleProp3.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp4 = value.DoubleProp4.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp5 = value.DoubleProp5.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp6 = value.DoubleProp6.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp7 = value.DoubleProp7.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp8 = value.DoubleProp8.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp9 = value.DoubleProp9.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp10 = value.DoubleProp10.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;

            _cacheSystem!.SetColumnOptionalModelFromView(value.Id, ref view);
        }
    }

    [Benchmark(Description = "Column SetFromView Optimised")]
    public void WriteInContextInternalFromView()
    {
        _cacheSystem!.WriteInContextColumnOptionalModel(static (in writeContext, state) =>
        {
            var valuesSpan = ((ModelWithOptions[])state).AsSpan();
            var view = new CacheSystem.SetViewColumnOptionalModel<ModelWithOptions>
            {
                Id = SetViewOperation.Set,
                FuncId = static (source) => source.Id,
                FuncDoubleProp1 = static (source) => source.DoubleProp1.HasValue ? source.DoubleProp1 : null,
                FuncDoubleProp2 = static (source) => source.DoubleProp2.HasValue ? source.DoubleProp2 : null,
                FuncDoubleProp3 = static (source) => source.DoubleProp3.HasValue ? source.DoubleProp3 : null,
                FuncDoubleProp4 = static (source) => source.DoubleProp4.HasValue ? source.DoubleProp4 : null,
                FuncDoubleProp5 = static (source) => source.DoubleProp5.HasValue ? source.DoubleProp5 : null,
                FuncDoubleProp6 = static (source) => source.DoubleProp6.HasValue ? source.DoubleProp6 : null,
                FuncDoubleProp7 = static (source) => source.DoubleProp7.HasValue ? source.DoubleProp7 : null,
                FuncDoubleProp8 = static (source) => source.DoubleProp8.HasValue ? source.DoubleProp8 : null,
                FuncDoubleProp9 = static (source) => source.DoubleProp9.HasValue ? source.DoubleProp9 : null,
                FuncDoubleProp10 = static (source) => source.DoubleProp10.HasValue ? source.DoubleProp10 : null,
            };

            for (int i = 0; i < valuesSpan.Length; i++)
            {
                var value = valuesSpan[i];

                view.Source = value;
                view.DoubleProp1 = value.DoubleProp1.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp2 = value.DoubleProp2.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp3 = value.DoubleProp3.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp4 = value.DoubleProp4.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp5 = value.DoubleProp5.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp6 = value.DoubleProp6.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp7 = value.DoubleProp7.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp8 = value.DoubleProp8.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp9 = value.DoubleProp9.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
                view.DoubleProp10 = value.DoubleProp10.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;

                writeContext.SetFromView(value.Id, ref view);
            }
        },
        _values!);
    }

    [Benchmark(Description = "Column SetFromView OptimisedPtr")]
    public void WriteInContextInternalFromViewPtr()
    {
        unsafe
        {
            _cacheSystem!.WriteInContextColumnOptionalModel(&SetFromViewPtrBody, _values!);
        }
    }

    private static void SetFromViewPtrBody(in CacheSystem.WriteContextColumnOptionalModel writeContext, object state)
    {
        var valuesSpan = ((ModelWithOptions[])state).AsSpan();
        var view = new CacheSystem.SetViewColumnOptionalModel<ModelWithOptions>
        {
            Id = SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            FuncDoubleProp1 = static (source) => source.DoubleProp1.HasValue ? source.DoubleProp1 : null,
            FuncDoubleProp2 = static (source) => source.DoubleProp2.HasValue ? source.DoubleProp2 : null,
            FuncDoubleProp3 = static (source) => source.DoubleProp3.HasValue ? source.DoubleProp3 : null,
            FuncDoubleProp4 = static (source) => source.DoubleProp4.HasValue ? source.DoubleProp4 : null,
            FuncDoubleProp5 = static (source) => source.DoubleProp5.HasValue ? source.DoubleProp5 : null,
            FuncDoubleProp6 = static (source) => source.DoubleProp6.HasValue ? source.DoubleProp6 : null,
            FuncDoubleProp7 = static (source) => source.DoubleProp7.HasValue ? source.DoubleProp7 : null,
            FuncDoubleProp8 = static (source) => source.DoubleProp8.HasValue ? source.DoubleProp8 : null,
            FuncDoubleProp9 = static (source) => source.DoubleProp9.HasValue ? source.DoubleProp9 : null,
            FuncDoubleProp10 = static (source) => source.DoubleProp10.HasValue ? source.DoubleProp10 : null,
        };

        for (int i = 0; i < valuesSpan.Length; i++)
        {
            var value = valuesSpan[i];

            view.Source = value;
            view.DoubleProp1 = value.DoubleProp1.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp2 = value.DoubleProp2.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp3 = value.DoubleProp3.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp4 = value.DoubleProp4.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp5 = value.DoubleProp5.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp6 = value.DoubleProp6.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp7 = value.DoubleProp7.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp8 = value.DoubleProp8.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp9 = value.DoubleProp9.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;
            view.DoubleProp10 = value.DoubleProp10.HasValue ? SetViewOperation.Set : SetViewOperation.RemoveIfExist;

            writeContext.SetFromView(value.Id, ref view);
        }
    }
}