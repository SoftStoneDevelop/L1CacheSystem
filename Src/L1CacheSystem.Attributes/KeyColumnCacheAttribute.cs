using L1CacheSystem.Attributes.Enums;
using System;

namespace L1CacheSystem.Attributes;

/// <summary>
/// Instruction for generating caching methods of a given type
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class KeyColumnCacheAttribute : Attribute
{
    /// <summary>
    /// Instruction for generating caching methods of a given type
    /// </summary>
    /// <param name="keyType">Type of key cached data</param>
    /// <param name="interfaceType">The type of interface that the generated code will become a part of</param>
    /// <param name="cacheName">Cache name—since types can repeat within the cache, even though the collections themselves are different</param>
    /// <param name="metricCollectFlags">Flags specifying which metrics to collect</param>
    public KeyColumnCacheAttribute(
        Type keyType,
        Type interfaceType,
        string cacheName,
        MetricCollectBits metricCollectFlags = MetricCollectBits.DefaultCollectBits)
    {
    }
}