using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Benchmarks.Backend;
using Benchmarks.Model;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;

namespace Benchmarks.Cases.Entity;

[MemoryDiagnoser]
[ThreadingDiagnoser]
[ExceptionDiagnoser]
[SimpleJob(RuntimeMoniker.Net10_0)]
public class SetMemoryCacheBench
{
    private FakeMeterFactory? _meterFactory;
    private CacheSystem? _cacheSystem;
    private MemoryCache? _memoryCache;

    [Params(10_000, 25_000, 50_000)]
    public int Size;

    [IterationSetup]
    public void IterationSetup()
    {
        _meterFactory = new FakeMeterFactory();
        _cacheSystem = new CacheSystem(NullLogger.Instance, TimeProvider.System, _meterFactory);
        _cacheSystem.StartAsync(default).Wait();

        _memoryCache = new MemoryCache(new MemoryCacheOptions(), NullLoggerFactory.Instance);
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        _cacheSystem?.StopAsync(default).Wait();
        _cacheSystem = null;
        _meterFactory = null;

        _memoryCache?.Dispose();
        _memoryCache = null;
    }

    [Benchmark(Description = "Set IMemoryCache", Baseline = true)]
    public async Task SetMemoryCache()
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
            _memoryCache!.Set(value.Id, value);
        }
    }

    [Benchmark(Description = "Set CacheSystem")]
    public async Task SetCacheSystem()
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
    }
}