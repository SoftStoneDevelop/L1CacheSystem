<h1 align="center">
  <a>L1CacheSystem</a>
</h1>

<h3 align="center">

  [![Nuget](https://img.shields.io/nuget/v/L1CacheSystem?logo=L1CacheSystem)](https://www.nuget.org/packages/L1CacheSystem/)
  [![Downloads](https://img.shields.io/nuget/dt/L1CacheSystem.svg)](https://www.nuget.org/packages/L1CacheSystem/)
  [![Nuget](https://img.shields.io/nuget/v/L1CacheSystem?logo=L1CacheSystem.Attributes)](https://www.nuget.org/packages/L1CacheSystem.Attributes/)
  [![Downloads](https://img.shields.io/nuget/dt/L1CacheSystem.svg)](https://www.nuget.org/packages/L1CacheSystem.Attributes/)
  [![Nuget](https://img.shields.io/nuget/v/L1CacheSystem?logo=L1CacheSystem.Generator)](https://www.nuget.org/packages/L1CacheSystem.Generator/)
  [![Downloads](https://img.shields.io/nuget/dt/L1CacheSystem.svg)](https://www.nuget.org/packages/L1CacheSystem.Generator/)
  [![Stars](https://img.shields.io/github/stars/SoftStoneDevelop/L1CacheSystem?color=brightgreen)](https://github.com/SoftStoneDevelop/L1CacheSystem/stargazers)
  [![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

</h3>

L1CacheSystem is the application’s internal cache. It was created to solve two problems that IMemoryCache couldn’t address:
 - The ability to filter by all cache entities
 - Blocking a specific resource by key
 
<h1 align="left">
  <a>Usage</a>
</h1>

The first thing you need to do is to import the packages:
- L1CacheSystem
- L1CacheSystem.Attributes;
- L1CacheSystem.Generator
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

Settings is done. Now usage case:
```csharp
public class MyValueService
{
  private readonly ILogger<MyValueService> _logger;
  private readonly IServiceCacheSystem _casheSystem;
  
  public MyValueService(ILogger<MyValueService> logger, IServiceCacheSystem casheSystem)
  {
    _logger = logger;
    _casheSystem = casheSystem;
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
    // if "expiry" is provided then casheSystem remove cache value from cache when key is expire

    // The Set method is thread‑safe on its own; the lockGuard is only needed if you’re using something more complex than a simple set/get.
  }

  public DoRelatedWork(long id, ILockGuard? lockGuard = null)
  {
    // in this method we not shure what poinc its called
    // maybe this is root call or maybe recudsive
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
