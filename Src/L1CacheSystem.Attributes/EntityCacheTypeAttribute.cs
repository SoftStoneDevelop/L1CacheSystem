using L1CacheSystem.Attributes.Enums;
using System;

namespace L1CacheSystem.Attributes;

/// <summary>
/// Instruction for generating caching methods of a given type
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class EntityCacheTypeAttribute : Attribute
{
    /// <summary>
    /// Instruction for generating caching methods of a given type
    /// </summary>
    /// <param name="cacheType">Type of cached data</param>
    /// <param name="interfaceType">The type of interface that the generated code will become a part of</param>
    /// <param name="cacheName">Cache name—since types can repeat within the cache, even though the collections themselves are different</param>
    /// <param name="propAsKey">The property from the type that should be used as a key</param>
    /// <param name="collectMetrics">Collecting metrics is quite time-consuming (2.5x overhead), so it is recommended to disable it if your application has strict response time requirements</param>
    /// <param name="copyMethodName">Name of the copy method</param>
    public EntityCacheTypeAttribute(
        Type cacheType,
        Type interfaceType,
        string cacheName,
        string propAsKey,
        MetricCollectBits metricCollectFlags = MetricCollectBits.DefaultCollectBits,
        string? copyMethodName = null,
        CopyMethodType copyMethodType = CopyMethodType.CopyMethodOnInstance)
    {
    }
}