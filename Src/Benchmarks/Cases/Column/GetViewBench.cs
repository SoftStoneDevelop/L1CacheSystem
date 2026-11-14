using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.Backend;
using Benchmarks.Model;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;

namespace Benchmarks.Cases.Column;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[SimpleJob(RuntimeMoniker.Net11_0)]
[SimpleJob(RuntimeMoniker.NativeAot11_0)]
public class GetViewBench
{
    private FakeMeterFactory? _meterFactory;
    private CacheSystem? _cacheSystem;

    [Params(10_000, 25_000, 50_000, 100_000)]
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
                    DoubleProp = i * 78451d,
                    Child = null
                } : null,
                Id = i,
                IntProp = i * 2,
                DoubleProp = haveOptional ? i * 8745d : null,
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

    [Benchmark(Description = "Entity CopyAll", Baseline = true)]
    public void EntityCopyAll()
    {
        var copyAll = _cacheSystem!.GetAllBitsNone(returnType: ReturnType.Copy);
    }

    [Benchmark(Description = "Column CopyAll")]
    public void ViewCopyAll()
    {
        var copyAll = new List<ValueClass>();
        _cacheSystem!.ViewInContextColumnCache(static (in viewContext, state) =>
        {
            var keys = viewContext.GetKeys();
            var copyAllCast = (List<ValueClass>)state;
            copyAllCast.Capacity = keys.Count;

            var getView = new CacheSystem.GetViewColumnCache<ValueClass>()
            {
                GetViewSourceType = GetViewSource.ByFunc,
                FuncCreateNewSource = static (exist) =>
                {
                    return exist ? new ValueClass() : null!;
                },

                Id = GetViewOperation.TryGet,
                ActionId = static (exists, value, source) =>
                {
                    if (exists)
                    {
                        source.Id = value;
                    }
                },

                IntProp = GetViewOperation.TryGet,
                ActionIntProp = static (exists, value, source) =>
                {
                    if (exists)
                    {
                        source.IntProp = value;
                    }
                },

                Child = GetViewOperation.TryGet,
                ActionChild = static (exists, value, source) =>
                {
                    if (exists)
                    {
                        source.Child = ValueClass.MakeCopy(value);
                    }
                },

                DoubleProp = GetViewOperation.TryGet,
                ActionDoubleProp = static (exists, value, source) =>
                {
                    if (exists)
                    {
                        source.DoubleProp = value;
                    }
                }
            };

            foreach (var key in keys)
            {
                viewContext.GetFromView(key, ref getView);
                if (getView.FuncCreateNewSourceCalled)
                {
                    copyAllCast.Add(getView.Source);
                }
            }
        },
        copyAll);
    }
}