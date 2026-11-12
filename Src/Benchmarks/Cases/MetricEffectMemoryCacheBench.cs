using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.Backend;
using Benchmarks.Model;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

namespace Benchmarks.Cases;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net11_0)]
[SimpleJob(RuntimeMoniker.NativeAot11_0)]
public class MetricEffectMemoryCacheBench
{
    private FakeMeterFactory? _meterFactory;
    private CacheSystem? _cacheSystem;

    [Params(10_000, 25_000, 50_000)]
    public int Size;

    [IterationSetup]
    public void IterationSetup()
    {
        _meterFactory = new FakeMeterFactory();
        var cacheSystem = new CacheSystem(NullLogger.Instance, TimeProvider.System, _meterFactory);
        _cacheSystem = cacheSystem;
        _cacheSystem.StartAsync(default).Wait();
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _cacheSystem?.StopAsync(default).Wait();

        _cacheSystem = null;
        _meterFactory = null;
    }

    [Benchmark(Description = "BitsNone", Baseline = true)]
    public async Task BitsNoneCase()
    {
        for (int i = 0; i < Size; i++)
        {
            var value = new ValueClass()
            {
                Child = new ValueClass()
                {
                    Id = i * 5,
                    IntProp = i * 3,
                    DoubleProp = i * 78451d,
                    Child = null
                },
                Id = i,
                IntProp = i * 2,
                DoubleProp = i * 8745d
            };

            _cacheSystem!.SetBitsNone(value, ValueOwnership.Transferred);
        }

        for (long i = 0; i < Size; i++)
        {
            var value = _cacheSystem!.GetBitsNone(i, ReturnType.Reference);
        }

        for (long i = 0; i < 100; i++)
        {
            var valueAll = _cacheSystem!.GetAllBitsNone(returnType: ReturnType.Reference);
        }

        for (long i = 0; i < Size; i++)
        {
            _cacheSystem!.DeleteBitsNone(i);
        }
    }

    [Benchmark(Description = "BitsDefault")]
    public async Task BitsDefaultCase()
    {
        for (int i = 0; i < Size; i++)
        {
            var value = new ValueClass()
            {
                Child = new ValueClass()
                {
                    Id = i * 5,
                    IntProp = i * 3,
                    DoubleProp = i * 78451d,
                    Child = null
                },
                Id = i,
                IntProp = i * 2,
                DoubleProp = i * 8745d
            };

            _cacheSystem!.SetBitsDefault(value, ValueOwnership.Transferred);
        }

        for (long i = 0; i < Size; i++)
        {
            var value = _cacheSystem!.GetBitsDefault(i, ReturnType.Reference);
        }

        for (long i = 0; i < 100; i++)
        {
            var valueAll = _cacheSystem!.GetAllBitsDefault(returnType: ReturnType.Reference);
        }

        for (long i = 0; i < Size; i++)
        {
            _cacheSystem!.DeleteBitsDefault(i);
        }
    }

    [Benchmark(Description = "BitsAllCount")]
    public async Task BitsAllCountCase()
    {
        for (int i = 0; i < Size; i++)
        {
            var value = new ValueClass()
            {
                Child = new ValueClass()
                {
                    Id = i * 5,
                    IntProp = i * 3,
                    DoubleProp = i * 78451d,
                    Child = null
                },
                Id = i,
                IntProp = i * 2,
                DoubleProp = i * 8745d
            };

            _cacheSystem!.SetBitsAllCount(value, ValueOwnership.Transferred);
        }

        for (long i = 0; i < Size; i++)
        {
            var value = _cacheSystem!.GetBitsAllCount(i, ReturnType.Reference);
        }

        for (long i = 0; i < 100; i++)
        {
            var valueAll = _cacheSystem!.GetAllBitsAllCount(returnType: ReturnType.Reference);
        }

        for (long i = 0; i < Size; i++)
        {
            _cacheSystem!.DeleteBitsAllCount(i);
        }
    }

    [Benchmark(Description = "BitsAllTimes")]
    public async Task BitsAllTimesCase()
    {
        for (int i = 0; i < Size; i++)
        {
            var value = new ValueClass()
            {
                Child = new ValueClass()
                {
                    Id = i * 5,
                    IntProp = i * 3,
                    DoubleProp = i * 78451d,
                    Child = null
                },
                Id = i,
                IntProp = i * 2,
                DoubleProp = i * 8745d
            };

            _cacheSystem!.SetBitsAllTimes(value, ValueOwnership.Transferred);
        }

        for (long i = 0; i < Size; i++)
        {
            var value = _cacheSystem!.GetBitsAllTimes(i, ReturnType.Reference);
        }

        for (long i = 0; i < 100; i++)
        {
            var valueAll = _cacheSystem!.GetAllBitsAllTimes(returnType: ReturnType.Reference);
        }

        for (long i = 0; i < Size; i++)
        {
            _cacheSystem!.DeleteBitsAllTimes(i);
        }
    }

    [Benchmark(Description = "BitsAll")]
    public async Task BitsAllCase()
    {
        for (int i = 0; i < Size; i++)
        {
            var value = new ValueClass()
            {
                Child = new ValueClass()
                {
                    Id = i * 5,
                    IntProp = i * 3,
                    DoubleProp = i * 78451d,
                    Child = null
                },
                Id = i,
                IntProp = i * 2,
                DoubleProp = i * 8745d
            };

            _cacheSystem!.SetBitsAll(value, ValueOwnership.Transferred);
        }

        for (long i = 0; i < Size; i++)
        {
            var value = _cacheSystem!.GetBitsAll(i, ReturnType.Reference);
        }


        for (long i = 0; i < 100; i++)
        {
            var valueAll = _cacheSystem!.GetAllBitsAll(returnType: ReturnType.Reference);
        }

        for (long i = 0; i < Size; i++)
        {
            _cacheSystem!.DeleteBitsAll(i);
        }
    }
}