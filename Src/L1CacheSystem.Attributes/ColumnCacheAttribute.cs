using L1CacheSystem.Attributes.Enums;
using System;

namespace L1CacheSystem.Attributes;

/// <summary>
/// Instruction for generating caching methods of a given type
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class ColumnCacheAttribute : Attribute
{
    /// <summary>
    /// Instruction for generating caching methods of a given type
    /// </summary>
    /// <param name="columnType">Type of column values</param>
    /// <param name="interfaceType">The type of interface that the generated code will become a part of</param>
    /// <param name="columnName">Column name</param>
    /// <param name="metricCollectFlags">Flags specifying which metrics to collect</param>
    public ColumnCacheAttribute(
        Type columnType,
        Type interfaceType,
        string columnName,
        MetricCollectBits metricCollectFlags = MetricCollectBits.DefaultCollectBits)
    {
    }
}