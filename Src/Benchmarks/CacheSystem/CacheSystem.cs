using Benchmarks.Model;
using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;

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

    public void SetCustomFromView<T>(long key, in SetViewColumnCache<T> viewInfo)
    {
        _columnCacheSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrAddDefault(_columnCacheIndexes, key, out _);
            if (viewInfo.Id != SetViewOperation.None)
            {
                var propIndex = index.GetIndex(0);
                if (propIndex == -1)
                {
                    if (viewInfo.Id == SetViewOperation.Set)
                    {
                        index.SetIndex(0, true, _columnCacheId.Count);
                        _columnCacheId.Add(viewInfo.GetId());
                        _columnCacheIdReverse.Add(key);
                    }
                }
                else
                {
                    if (viewInfo.Id == SetViewOperation.Set)
                    {
                        _columnCacheId[propIndex] = viewInfo.GetId();
                    }
                    else
                    {
                        InternalDeleteColumnCacheId(0);
                    }
                }
            }

            if (viewInfo.IntProp != SetViewOperation.None)
            {
                var propIndex = index.GetIndex(1);
                if (propIndex == -1)
                {
                    if (viewInfo.IntProp == SetViewOperation.Set)
                    {
                        index.SetIndex(1, true, _columnCacheIntProp.Count);
                        _columnCacheIntProp.Add(viewInfo.GetIntProp());
                        _columnCacheIntPropReverse.Add(key);
                    }
                }
                else
                {
                    if (viewInfo.IntProp == SetViewOperation.Set)
                    {
                        _columnCacheIntProp[propIndex] = viewInfo.GetIntProp();
                    }
                    else
                    {
                        InternalDeleteColumnCacheIntProp(1);
                    }
                }
            }

            if (viewInfo.DoubleProp != SetViewOperation.None)
            {
                var propIndex = index.GetIndex(2);
                if (propIndex == -1)
                {
                    if (viewInfo.DoubleProp == SetViewOperation.Set)
                    {
                        index.SetIndex(2, true, _columnCacheDoubleProp.Count);
                        _columnCacheDoubleProp.Add(viewInfo.GetDoubleProp());
                        _columnCacheDoublePropReverse.Add(key);
                    }
                }
                else
                {
                    if (viewInfo.DoubleProp == SetViewOperation.Set)
                    {
                        _columnCacheDoubleProp[propIndex] = viewInfo.GetDoubleProp();
                    }
                    else
                    {
                        InternalDeleteColumnCacheDoubleProp(2);
                    }
                }
            }

            if (viewInfo.Child != SetViewOperation.None)
            {
                var propIndex = index.GetIndex(3);
                if (propIndex == -1)
                {
                    if (viewInfo.Child == SetViewOperation.Set)
                    {
                        index.SetIndex(3, true, _columnCacheChild.Count);
                        _columnCacheChild.Add(viewInfo.GetChild());
                        _columnCacheChildReverse.Add(key);
                    }
                }
                else
                {
                    if (viewInfo.Child == SetViewOperation.Set)
                    {
                        _columnCacheChild[propIndex] = viewInfo.GetChild();
                    }
                    else
                    {
                        InternalDeleteColumnCacheChild(3);
                    }
                }
            }
        }
        finally
        {
            _columnCacheSlimLock.ExitWriteLock();
        }
    }
}