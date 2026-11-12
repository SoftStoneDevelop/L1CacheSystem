using Benchmarks.Model;
using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using L1CacheSystem.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;
using System.Threading;

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

    public void ForeachBitsNone(Action<long, Benchmarks.Model.ValueClass> action)
    {
        foreach (var pair in _bitsNoneCollection)
        {
            action(pair.Key, pair.Value.Value);
        }
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
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_columnCacheIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();
                if (viewInfo.Id == SetViewOperation.Set)
                {
                    si.SetIndex(0, true, _columnCacheId.Count);
                    _columnCacheId.Add(viewInfo.GetId());
                    _columnCacheIdReverse.Add(key);
                }

                if (viewInfo.IntProp == SetViewOperation.Set)
                {
                    si.SetIndex(1, true, _columnCacheIntProp.Count);
                    _columnCacheIntProp.Add(viewInfo.GetIntProp());
                    _columnCacheIntPropReverse.Add(key);
                }

                if (viewInfo.DoubleProp == SetViewOperation.Set)
                {
                    si.SetIndex(2, true, _columnCacheDoubleProp.Count);
                    _columnCacheDoubleProp.Add(viewInfo.GetDoubleProp());
                    _columnCacheDoublePropReverse.Add(key);
                }

                if (viewInfo.Child == SetViewOperation.Set)
                {
                    si.SetIndex(3, true, _columnCacheChild.Count);
                    _columnCacheChild.Add(viewInfo.GetChild());
                    _columnCacheChildReverse.Add(key);
                }

                _columnCacheIndexes.Add(key, si);
            }
            else
            {
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
                            if (propIndex == _columnCacheId.Count - 1)
                            {
                                _columnCacheId.RemoveAt(propIndex);
                                _columnCacheIdReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _columnCacheId.Count - 1;
                                var swapId = _columnCacheIdReverse[endOfListIndex];
                                _columnCacheIdReverse[propIndex] = swapId;
                                _columnCacheIdReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_columnCacheIndexes, swapId);
                                swapIndex.SetIndex(0, false, propIndex);
                                _columnCacheId[propIndex] = _columnCacheId[endOfListIndex];
                                _columnCacheId.RemoveAt(endOfListIndex);
                            }
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
                            if (propIndex == _columnCacheIntProp.Count - 1)
                            {
                                _columnCacheIntProp.RemoveAt(propIndex);
                                _columnCacheIntPropReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _columnCacheIntProp.Count - 1;
                                var swapId = _columnCacheIntPropReverse[endOfListIndex];
                                _columnCacheIntPropReverse[propIndex] = swapId;
                                _columnCacheIntPropReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_columnCacheIndexes, swapId);
                                swapIndex.SetIndex(1, false, propIndex);
                                _columnCacheIntProp[propIndex] = _columnCacheIntProp[endOfListIndex];
                                _columnCacheIntProp.RemoveAt(endOfListIndex);
                            }
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
                            if (propIndex == _columnCacheDoubleProp.Count - 1)
                            {
                                _columnCacheDoubleProp.RemoveAt(propIndex);
                                _columnCacheDoublePropReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _columnCacheDoubleProp.Count - 1;
                                var swapId = _columnCacheDoublePropReverse[endOfListIndex];
                                _columnCacheDoublePropReverse[propIndex] = swapId;
                                _columnCacheDoublePropReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_columnCacheIndexes, swapId);
                                swapIndex.SetIndex(2, false, propIndex);
                                _columnCacheDoubleProp[propIndex] = _columnCacheDoubleProp[endOfListIndex];
                                _columnCacheDoubleProp.RemoveAt(endOfListIndex);
                            }
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
                            if (propIndex == _columnCacheChild.Count - 1)
                            {
                                _columnCacheChild.RemoveAt(propIndex);
                                _columnCacheChildReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _columnCacheChild.Count - 1;
                                var swapId = _columnCacheChildReverse[endOfListIndex];
                                _columnCacheChildReverse[propIndex] = swapId;
                                _columnCacheChildReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_columnCacheIndexes, swapId);
                                swapIndex.SetIndex(3, false, propIndex);
                                _columnCacheChild[propIndex] = _columnCacheChild[endOfListIndex];
                                _columnCacheChild.RemoveAt(endOfListIndex);
                            }
                        }
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