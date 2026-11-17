using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.Backend;
using Benchmarks.Model;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Numerics.Tensors;
using System.Threading.Tasks;

namespace Benchmarks.Cases.Column;

[MemoryDiagnoser]
[HardwareCounters]
[SimpleJob(RuntimeMoniker.Net11_0)]
[SimpleJob(RuntimeMoniker.NativeAot11_0)]
public class ViewInContextBench
{
    private FakeMeterFactory? _meterFactory;
    private CacheSystem? _cacheSystem;

    [Params(10_000, 50_000, 100_000, 1_000_000)]
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

            _cacheSystem!.SetColumnCacheFromView(value.Id, ref view);
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

    [Benchmark(Description = "Entity Foreach", Baseline = true)]
    public async Task ForeachEntity()
    {
        var sum = 0;
        _cacheSystem!.ForeachBitsNone((value) => { sum += value.IntProp; });
    }

    [Benchmark(Description = "ViewInContext naive")]
    public async Task ViewInContextNaive()
    {
        var sum = 0;
        _cacheSystem!.ViewInContextColumnCache((in context) =>
        {
            var span = context.GetIntPropSpan();
            for (int i = 0; i < span.Length; i++)
            {
                sum += span[i];
            }
        });
    }

    [Benchmark(Description = "ViewInContext Optimised")]
    public async Task ViewInContextOptimised()
    {
        var sum = 0;
        _cacheSystem!.ViewInContextColumnCache(static (in viewContext, ref refSum) =>
        {
            var span = viewContext.GetIntPropSpan();
            var interSum = 0;
            for (int i = 0; i < span.Length; i++)
            {
                interSum += span[i];
            }

            refSum = interSum;
        },
        ref sum);
    }

    [Benchmark(Description = "ViewInContext OptimisedPtr")]
    public async Task ViewInContextOptimisedPtr()
    {
        var sum = 0;
        unsafe
        {
            _cacheSystem!.ViewInContextColumnCache(&OptimisedPtrBody, ref sum);
        }
    }

    private static void OptimisedPtrBody(in CacheSystem.ViewContextColumnCache viewContext, ref int refSum)
    {
        var span = viewContext.GetIntPropSpan();
        var interSum = 0;
        for (int i = 0; i < span.Length; i++)
        {
            interSum += span[i];
        }

        refSum = interSum;
    }

    [Benchmark(Description = "ViewInContext OptimisedPtr Tensor")]
    public async Task ViewInContextOptimisedPtrTensor()
    {
        var sum = 0;
        unsafe
        {
            _cacheSystem!.ViewInContextColumnCache(&OptimisedPtrTensorBody, ref sum);
        }
    }

    private static void OptimisedPtrTensorBody(in CacheSystem.ViewContextColumnCache viewContext, ref int refSum)
    {
        refSum = TensorPrimitives.Sum(viewContext.GetIntPropSpan());
    }
}