using Benchmarks.Model;
using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Metrics;

namespace Benchmarks.Backend;

[EntityCacheType(
        cacheType: typeof(ValueClass),
        interfaceType: typeof(ICacheSystem),
        cacheName: "BitsNone",
        propAsKey: nameof(ValueClass.Id),
        metricCollectFlags: MetricCollectBits.None,
        copyMethodName: nameof(ValueClass.MakeCopy),
        copyMethodType: CopyMethodType.CopyMethodOnType)]
[EntityCacheType(
        cacheType: typeof(ValueClass),
        interfaceType: typeof(ICacheSystem),
        cacheName: "BitsDefault",
        propAsKey: nameof(ValueClass.Id),
        metricCollectFlags: MetricCollectBits.DefaultCollectBits,
        copyMethodName: nameof(ValueClass.MakeCopy),
        copyMethodType: CopyMethodType.CopyMethodOnType)]
[EntityCacheType(
        cacheType: typeof(ValueClass),
        interfaceType: typeof(ICacheSystem),
        cacheName: "BitsAllTimes",
        propAsKey: nameof(ValueClass.Id),
        metricCollectFlags: MetricCollectBits.AllTimes,
        copyMethodName: nameof(ValueClass.MakeCopy),
        copyMethodType: CopyMethodType.CopyMethodOnType)]
[EntityCacheType(
        cacheType: typeof(ValueClass),
        interfaceType: typeof(ICacheSystem),
        cacheName: "BitsAllCount",
        propAsKey: nameof(ValueClass.Id),
        metricCollectFlags: MetricCollectBits.AllCount,
        copyMethodName: nameof(ValueClass.MakeCopy),
        copyMethodType: CopyMethodType.CopyMethodOnType)]
[EntityCacheType(
        cacheType: typeof(ValueClass),
        interfaceType: typeof(ICacheSystem),
        cacheName: "BitsAll",
        propAsKey: nameof(ValueClass.Id),
        metricCollectFlags: MetricCollectBits.AllBits,
        copyMethodName: nameof(ValueClass.MakeCopy),
        copyMethodType: CopyMethodType.CopyMethodOnType)]
[KeyColumnCache(
    keyType: typeof(long),
    interfaceType: typeof(ICacheSystem),
    cacheName: "ColumnCache"),
    ColumnCache(
    columnType: typeof(long),
    columnName: nameof(ValueClass.Id)),
    ColumnCache(
    columnType: typeof(int),
    columnName: nameof(ValueClass.IntProp)),
    ColumnCache(
    columnType: typeof(double?),
    columnName: nameof(ValueClass.DoubleProp)),
    ColumnCache(
    columnType: typeof(ValueClass),
    columnName: nameof(ValueClass.Child))]
public sealed partial class CacheSystem : L1CacheSystem.L1CacheSystemBase, ICacheSystem
{
    public CacheSystem(
        ILogger logger,
        TimeProvider timeProvider,
        IMeterFactory meterFactory)
        : base(logger, timeProvider, meterFactory)
    {
    }

    public void ForeachBitsNone(Action<Benchmarks.Model.ValueClass> action)
    {
        foreach (var value in _bitsNoneCollection.Values)
        {
            action(value.Value);
        }
    }

    public delegate void ViewContextColumnCacheStateAction<T, Q>(in CacheSystem.ViewContextColumnCache context, ref T state, ref Q state2);

    public void ViewInContextColumnCache<T, Q>(CacheSystem.ViewContextColumnCacheStateAction<T, Q> action, ref T state, ref Q state2)
    {
        _columnCacheSlimLock.EnterReadLock();
        try
        {
            var context = new ViewContextColumnCache(this);
            action(in context, ref state, ref state2);
        }
        finally
        {
            _columnCacheSlimLock.ExitReadLock();
        }
    }
}