<h1 align="center">
  <a>L1CacheSystem</a>
</h1>

<h3 align="center">

  [![Nuget](https://img.shields.io/nuget/v/L1CacheSystem?logo=L1CacheSystem)](https://www.nuget.org/packages/L1CacheSystem/)
  [![Downloads](https://img.shields.io/nuget/dt/L1CacheSystem.svg)](https://www.nuget.org/packages/L1CacheSystem/)
  [![Nuget](https://img.shields.io/nuget/v/L1CacheSystem?logo=L1CacheSystem.Attributes)](https://www.nuget.org/packages/L1CacheSystem.Attributes/)
  [![Downloads](https://img.shields.io/nuget/dt/L1CacheSystem.Attributes.svg)](https://www.nuget.org/packages/L1CacheSystem.Attributes/)
  [![Nuget](https://img.shields.io/nuget/v/L1CacheSystem?logo=L1CacheSystem.Generator)](https://www.nuget.org/packages/L1CacheSystem.Generator/)
  [![Downloads](https://img.shields.io/nuget/dt/L1CacheSystem.Generator.svg)](https://www.nuget.org/packages/L1CacheSystem.Generator/)
  [![Stars](https://img.shields.io/github/stars/SoftStoneDevelop/L1CacheSystem?color=brightgreen)](https://github.com/SoftStoneDevelop/L1CacheSystem/stargazers)
  [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

</h3>

L1CacheSystem is the application’s internal cache. It was created to solve two problems that IMemoryCache couldn’t address:
 - The ability to filter by all cache entities
 - Blocking a specific resource by key

Caching is also supported in two modes:
 - Entities
 - Columns

An in-app cache system. It all started because `IMemoryCache` doesn’t allow you to scan a collection (and I really wanted to. Yeah, yeah, some might say that’s an anti-pattern and that I’m a total degenerate, but in the context where I used it, that’s exactly what was needed). That led to the first part of the EntityCache system, which is similar to `IMemoryCache` in terms of its API. It also supports automatic copying upon retrieval or returning a reference if a copy isn’t needed.

At some point, I became dissatisfied with the cache’s scanning speed; moreover, stored entities sometimes have a lot of optional properties, but if you store the classes, memory is allocated for everything anyway. This led to the introduction of the ability to store data in the cache by columns, which allows for column-based scanning—a process that’s faster than reading separate classes, since columns are stored sequentially in memory.

Both caches support time-based expiration and the ability to lock individual keys in the cache (as if you had RedLock built into the application. This is sometimes necessary to support complex business logic).

Metrics are available for all operations; if metrics are not needed, they can be disabled either for the entire cache or for a specific one. Enabling/disabling is done via flags for each type separately. If a metric is not enabled, it does not exist in the code at all—the code generator simply won’t add it.

[Benchmarks](https://github.com/SoftStoneDevelop/L1CacheSystem/blob/main/Benchmarks.md)

<h1 align="left">
  <a>Usage</a>
</h1>

The first thing you need to do is to import the packages:
- `L1CacheSystem`
- `L1CacheSystem.Attributes`
- `L1CacheSystem.Generator`

Then, in the code, you need to declare your cache. Let’s say that, for example, we will cache the `MyValue` class.
```csharp
public class MyValue
{
  public long Id { get; set; }

  public double Value { get; set; }
  // other properties

  public MyValue Copy()
  {
    return new MyValue
    {
      Id = this.Id,
      Value = this.Value
    };
  }
}
```

```csharp

public partial interface IServiceCacheSystem
{
}

// You can have as many EntityCacheType as you like, the number of different types is not limited.
[EntityCacheType(
    cacheType: typeof(MyValue),
    interfaceType: typeof(IServiceCacheSystem),
    cacheName: "YouCacheName",
    propAsKey: nameof(MyValue.Id),
    metricCollectFlags: MetricCollectBits.AllBits, // what metrics for this specific cache need to collect
    copyMethodName: nameof(MyValue.Copy),
    copyMethodType: CopyMethodType.CopyMethodOnType)] // we also support: static method and copy-constructor
[KeyColumnCache(
    keyType: typeof(long), // In column mode, the key is a separate entity from the columns.
    interfaceType: typeof(IServiceCacheSystem),
    cacheName: "YouColumnCacheName"),
    ColumnCache(columnType: typeof(long), columnName: nameof(MyValue.Id)),
    ColumnCache(columnType: typeof(double), columnName: nameof(MyValue.Value))] // It is recommended to have no more than 32 columns for each KeyColumnCache.
public partial class ServiceCacheSystem : L1CacheSystemBase, IServiceCacheSystem
{
    public ServiceCacheSystem(
        ILogger<ServiceCacheSystem> logger,
        TimeProvider timeProvider,
        IMeterFactory meterFactory)
        : base(
            logger,
            timeProvider,
            meterFactory)
    {
    }
}
```

Now you just need create and run system. For example in asp.net we can do this with that way:
```csharp
public class CacheSystemHostedService : IHostedService
{
    private readonly IL1CacheSystemBase _cacheSystem;

    public CacheSystemHostedService(IL1CacheSystemBase cacheSystem)
    {
        _cacheSystem = cacheSystem;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _cacheSystem.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _cacheSystem.StopAsync(cancellationToken);
    }
}
```

```csharp
services.AddSingleton<ServiceCacheSystem>();
services.AddSingleton<IL1CacheSystemBase>((provider) => provider.GetRequiredService<ServiceCacheSystem>()); // IL1CacheSystemBase interface providet by L1CacheSystem nuget
services.AddSingleton<IServiceCacheSystem>((provider) => provider.GetRequiredService<ServiceCacheSystem>());
```

Settings is done.

Now usage case (Entity):
```csharp
public class MyValueService
{
  private readonly ILogger<MyValueService> _logger;
  private readonly IServiceCacheSystem _cacheSystem;
  
  public MyValueService(ILogger<MyValueService> logger, IServiceCacheSystem cacheSystem)
  {
    _logger = logger;
    _cacheSystem = cacheSystem;
  }

  public CreateValue(long id)
  {
    await using var lockGuard = new LockGuard(logger: _logger, parent: null);

    // Since we are at the root of the call (parent == null), we will either gain ownership of the resource immediately,
    // or at the moment when other calls stop owning it. If the waiting should not be infinite, pass a CancellationToken.
    await _cacheSystem.GetYouCacheNameLockAsync(id, lockGuard, default);

    // do whatever you want: your custom logic
    //  YouCacheName with Id = id nobody (except you) can change until lockGuard.Dispose not called
    var value = new MyValue() { Id = id, Value = 7114 };

    // If you need the value and plan to change it after this call, then pass it as `NotTransferred`
    // Then CacheSystem will save a copy of it during the call (by copying it using the method you specified earlier, at the configuration stage).

    _cacheSystem.SetYouCacheName(value.Id, value, valueOwnership: ValueOwnership.Transferred, expiry: null);
    // if "expiry" is provided then cacheSystem remove cache value from cache when key is expire

    // The Set method is thread‑safe on its own; the lockGuard is only needed if you’re using something more complex than a simple set/get.
  }

  public DoRelatedWork(long id, ILockGuard? lockGuard = null)
  {
    // in this method we not shure what poinc its called
    // maybe this is root call or maybe recursive
    await using var lockGuard = new LockGuard(_logger, lockGuard);

    // If this is not the root (lockGuard != null) and the parent already has a lock on this key,
    // then the lock counter inside lockGuard will simply be increased.
    await _cacheSystem.GetYouCacheNameLockAsync(id, lockGuard, default);

    // your custom logic

    // On dispose lockGuard release all locks: decrease lock counter if this lockGuard not the owner of lock or release if is the owner
  }

  public MyValue GetMyValue(long id)
  {
    return _cacheSystem.GetYouCacheName(id, ReturnType.Reference); // or ReturnType.Copy
  }

  public MyValue[] GetAllMyValue(long id, Func<MyValue, bool>? predicate = null)
  {
    return _cacheSystem.GetAllYouCacheName(predicate, ReturnType.Reference); // or ReturnType.Copy
  }
}
```

Now usage case (Column):
```csharp
public class MyValueService
{
  private readonly ILogger<MyValueService> _logger;
  private readonly IServiceCacheSystem _cacheSystem;
  
  public MyValueService(ILogger<MyValueService> logger, IServiceCacheSystem cacheSystem)
  {
    _logger = logger;
    _cacheSystem = cacheSystem;
  }

  public CreateValue(long id)
  {
    await using var lockGuard = new LockGuard(logger: _logger, parent: null);

    await _cacheSystem.GetYouCacheNameLockAsync(id, lockGuard, default);
    var value = new MyValue() { Id = id, Value = 7114 };

    var view = new CacheSystem.SetViewYouColumnCacheName<MyValue> // You can fill it in from anywhere; you just need to create a view and fill in its fields.
    {
        Id = SetViewOperation.Set,
        FuncId = static (source) => source.Id,
        Value = SetViewOperation.Set,
        FuncValue = static (source) => source.IntProp
    };

    _cacheSystem.SetYouColumnCacheNameFromView(value.Id, view, expiry: null);

    // You can also fill in and update not everything at once, but column by column.
    // _cacheSystem.SetColumnCacheId(value.Id, value.Id, expiry: null);
    // _cacheSystem.SetColumnCacheId(value.Id, value.Value, expiry: null);
  }

  public double GetValue(long id)
  {
    var haveValue = _cacheSystem.TryGetYouColumnCacheNameValue(id, out var value);
    // haveValue
    // There may be no columns at all, since columns are just a promise that they might exist, not a requirement. Each entity may or may not have them.

    return value; // We have a simple example, so we’re definitely sure that it’s there.
  }

  public double SumValue()
  {
    double sum;
    _cacheSystem.ViewInContextYouColumnCacheName((context) =>
	{
		var span = context.GetValueSpan();
		for (int i = 0; i < span.Length; i++)
		{
			sum += span[i];
		}
	});

    return sum;
  }
}
```

What metrics look like, using Grafana visualization as an example:
<img width="2058" height="1147" alt="image" src="https://github.com/user-attachments/assets/96e0eff2-92b0-4958-a7b4-0268a9091a47" />
<img width="2062" height="1114" alt="image" src="https://github.com/user-attachments/assets/8fac71c5-ead0-412a-8b98-9d67df48cdc6" />
<img width="2068" height="1099" alt="image" src="https://github.com/user-attachments/assets/671f238c-366b-4b32-b590-780e317c7c5e" />
<img width="2066" height="1112" alt="image" src="https://github.com/user-attachments/assets/e916b4fa-e05f-4b91-81d7-4d13d0fee8af" />
<img width="2071" height="604" alt="image" src="https://github.com/user-attachments/assets/b39d5efd-5a2d-4427-9383-4f84d4bcd15d" />
<img width="2067" height="613" alt="image" src="https://github.com/user-attachments/assets/0ad45a57-2dd1-4087-aaf0-ce8731d3dc1b" />
