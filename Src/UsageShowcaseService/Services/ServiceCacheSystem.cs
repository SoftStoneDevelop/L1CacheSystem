using L1CacheSystem;
using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Metrics;
using UsageShowcaseService.Dtos;
using UsageShowcaseService.Services.Interfaces;
using UsageShowcaseService.Values;

namespace UsageShowcaseService.Services;

[EntityCacheType(
    cacheType: typeof(ScreamValue),
    interfaceType: typeof(IServiceCacheSystem),
    cacheName: "Scream",
    propAsKey: nameof(ScreamValue.Id),
    metricCollectFlags: MetricCollectBits.AllBits,
    copyMethodName: nameof(ScreamValue.CopyScream),
    copyMethodType: CopyMethodType.CopyMethodOnType)]
[EntityCacheType(
    cacheType: typeof(EchoValue),
    interfaceType: typeof(IServiceCacheSystem),
    cacheName: "Echo",
    propAsKey: nameof(EchoValue.Id),
    metricCollectFlags: MetricCollectBits.AllBits,
    copyMethodName: nameof(EchoValue.Copy),
    copyMethodType: CopyMethodType.CopyMethodOnInstance)]
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