using Benchmarks.Model;
using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
public sealed partial class CacheSystem : L1CacheSystem.L1CacheSystemBase, ICacheSystem
{
    public CacheSystem(
        ILogger logger,
        TimeProvider timeProvider,
        IMeterFactory meterFactory)
        : base(logger, timeProvider, meterFactory)
    {
    }

    private ReaderWriterLockSlim _customSlimLock = new ReaderWriterLockSlim();

    private readonly Dictionary<long, SparseIndex> _customIndexes;

    private readonly List<long> _customIdPropertyReverse;
    private readonly List<long> _customIdProperty;

    private readonly List<long> _customIntPropPropertyReverse;
    private readonly List<int> _customIntPropProperty;

    private readonly List<long> _customDoublePropPropertyReverse;
    private readonly List<double> _customDoublePropProperty;

    private readonly List<long> _customChildPropertyReverse;
    private readonly List<ValueClass> _customChildProperty;

    public void SetCustom(int key, ValueClass value)
    {
        var id = value.Id;
        var intProp = value.IntProp;
        var doubleProp = value.DoubleProp;
        var child = value.Child;

        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();
                si.SetIndex(0, true, _customIdProperty.Count - 1);
                _customIdProperty.Add(id);
                _customIdPropertyReverse.Add(key);

                si.SetIndex(1, true, _customIntPropProperty.Count - 1);
                _customIntPropProperty.Add(intProp);
                _customIntPropPropertyReverse.Add(key);

                si.SetIndex(2, true, _customDoublePropProperty.Count - 1);
                _customDoublePropProperty.Add(doubleProp);
                _customDoublePropPropertyReverse.Add(key);

                var haveChild = child != null;
                si.SetIndex(3, haveChild, _customChildProperty.Count - 1);
                if (haveChild)
                {
                    _customChildProperty.Add(child);
                    _customChildPropertyReverse.Add(key);
                }

                _customIndexes.Add(id, si);
            }
            else
            {
                _customIdProperty[index.GetIndex(0)] = id;
                _customIntPropProperty[index.GetIndex(1)] = intProp;
                _customDoublePropProperty[index.GetIndex(2)] = doubleProp;

                var haveChild = child != null;
                var childOldIndex = index.GetIndex(3);
                if (haveChild)
                {
                    if (childOldIndex == -1)
                    {
                        index.SetIndex(3, true, _customChildProperty.Count - 1);
                        _customChildProperty.Add(child);
                    }
                    else
                    {
                        index.SetIndex(3, true, childOldIndex);
                        _customChildProperty[childOldIndex] = child;
                    }
                }
                else
                {
                    if (childOldIndex == -1)
                    {
                        // nothing
                    }
                    else
                    {
                        index.SetIndex(3, false, childOldIndex);
                        if (childOldIndex == _customChildProperty.Count - 1)
                        {
                            _customChildProperty.RemoveAt(childOldIndex);
                        }
                        else
                        {
                            ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, .Id);
                            _customChildProperty[childOldIndex] = _customChildProperty[^1];
                        }
                    }
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }

    public void DeleteCustom(long id)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            if (!_customIndexes.Remove(id, out var index))
            {
                return;
            }

            if (index + 1 == _customIdProperty.Count)
            {
                _customIdProperty.RemoveAt(index);
                _customIntPropProperty.RemoveAt(index);
                _customDoublePropProperty.RemoveAt(index);
                _customChildProperty.RemoveAt(index);
            }
            else
            {
                var count = _customIdProperty.Count;
                var swapId = _customIdProperty[^1];
                _customIdProperty[index] = swapId;
                _customIdProperty.RemoveAt(count - 1);
                _customIndexes[swapId] = index;

                _customIntPropProperty[index] = _customIntPropProperty[^1];
                _customIntPropProperty.RemoveAt(count - 1);

                _customDoublePropProperty[index] = _customDoublePropProperty[^1];
                _customDoublePropProperty.RemoveAt(count - 1);

                _customChildProperty[index] = _customChildProperty[^1];
                _customChildProperty.RemoveAt(count - 1);
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }
}