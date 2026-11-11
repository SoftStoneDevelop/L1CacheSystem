using Benchmarks.Model;
using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using L1CacheSystem.Enums;
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

    public void ForeachBitsNone(Action<long, Benchmarks.Model.ValueClass> action)
    {
        foreach (var pair in _bitsNoneCollection)
        {
            action(pair.Key, pair.Value.Value);
        }
    }

    private readonly Dictionary<long, SparseIndex> _customIndexes = [];

    private readonly ReaderWriterLockSlim _customSlimLock = new();
    private readonly List<long> _customIdPropertyReverse = [];
    private readonly List<long> _customIdProperty = [];

    public void SetCustomFromView(long key, ValueClass value)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index)) // add new
            {
                var si = new SparseIndex();

                si.SetIndex(0, true, _customIdProperty.Count);
                _customIdProperty.Add(value.Id);
                _customIdPropertyReverse.Add(key);

                si.SetIndex(1, true, _customIntPropProperty.Count);
                _customIntPropProperty.Add(value.IntProp);
                _customIntPropPropertyReverse.Add(key);

                var haveDoubleProp = value.DoubleProp.HasValue;
                si.SetIndex(2, haveDoubleProp, _customDoublePropProperty.Count);
                if (haveDoubleProp)
                {
                    _customDoublePropProperty.Add(value.DoubleProp.Value);
                    _customDoublePropPropertyReverse.Add(key);
                }

                var haveChild = value.Child != null;
                si.SetIndex(3, haveChild, _customChildProperty.Count);
                if (haveChild)
                {
                    _customChildProperty.Add(value.Child);
                    _customChildPropertyReverse.Add(key);
                }

                _customIndexes.Add(key, si);
            }
            else // update
            {
                _customIdProperty[index.GetIndex(0)] = value.Id;

                _customIntPropProperty[index.GetIndex(1)] = value.IntProp;

                var haveDoubleProp = value.DoubleProp.HasValue;
                var doublePropOldIndex = index.GetIndex(2);
                if (haveDoubleProp)
                {
                    if (doublePropOldIndex == -1)
                    {
                        index.SetIndex(2, true, _customDoublePropProperty.Count - 1);
                        _customDoublePropProperty.Add(value.DoubleProp.Value);
                    }
                    else
                    {
                        index.SetIndex(2, true, doublePropOldIndex);
                        _customDoublePropProperty[doublePropOldIndex] = value.DoubleProp.Value;
                    }
                }
                else if (doublePropOldIndex != -1)
                {
                    index.SetIndex(2, false, doublePropOldIndex);
                    if (doublePropOldIndex == _customDoublePropProperty.Count - 1)
                    {
                        _customDoublePropProperty.RemoveAt(doublePropOldIndex);
                        _customDoublePropPropertyReverse.RemoveAt(doublePropOldIndex);
                    }
                    else
                    {
                        var endOfListIndex = _customDoublePropProperty.Count - 1;
                        var swapId = _customDoublePropPropertyReverse[endOfListIndex];
                        _customDoublePropPropertyReverse[doublePropOldIndex] = swapId;
                        _customDoublePropPropertyReverse.RemoveAt(endOfListIndex);

                        ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                        swapIndex.SetIndex(2, false, doublePropOldIndex);
                        _customDoublePropProperty[doublePropOldIndex] = _customDoublePropProperty[endOfListIndex];
                        _customDoublePropProperty.RemoveAt(doublePropOldIndex);
                    }
                }

                var haveChild = value.Child != null;
                var childOldIndex = index.GetIndex(3);
                if (haveChild)
                {
                    if (childOldIndex == -1)
                    {
                        index.SetIndex(3, true, _customChildProperty.Count);
                        _customChildProperty.Add(value.Child);
                    }
                    else
                    {
                        index.SetIndex(3, true, childOldIndex);
                        _customChildProperty[childOldIndex] = value.Child;
                    }
                }
                else if (childOldIndex != -1)
                {
                    index.SetIndex(3, false, childOldIndex);
                    if (childOldIndex == _customChildProperty.Count - 1)
                    {
                        _customChildProperty.RemoveAt(childOldIndex);
                        _customChildPropertyReverse.RemoveAt(childOldIndex);
                    }
                    else
                    {
                        var endOfListIndex = _customChildProperty.Count - 1;
                        var swapId = _customChildPropertyReverse[endOfListIndex];
                        _customChildPropertyReverse[childOldIndex] = swapId;
                        _customChildPropertyReverse.RemoveAt(endOfListIndex);

                        ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                        swapIndex.SetIndex(3, false, childOldIndex);
                        _customChildProperty[childOldIndex] = _customChildProperty[endOfListIndex];
                        _customChildProperty.RemoveAt(childOldIndex);
                    }
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }

    public bool TryGetCustomId(long key, out long idProp)
    {
        _customSlimLock.EnterReadLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                idProp = 0;
                return false;
            }
            else
            {
                var listIndex = index.GetIndex(0);
                if (listIndex == -1)
                {
                    idProp = 0;
                    return false;
                }
                else
                {
                    idProp = _customIdProperty[listIndex];
                    return true;
                }
            }
        }
        finally
        {
            _customSlimLock.ExitReadLock();
        }
    }

    public void ForeachByIntPropColumn(Action<long, int> intPropAction)
    {
        _customSlimLock.EnterReadLock();
        try
        {
            var spanIntProp = CollectionsMarshal.AsSpan(_customIntPropProperty);
            var spanIntPropReverse = CollectionsMarshal.AsSpan(_customIntPropPropertyReverse);
            for (int i = 0; i < spanIntProp.Length; i++)
            {
                intPropAction(spanIntPropReverse[i], spanIntProp[i]);
            }
        }
        finally
        {
            _customSlimLock.ExitReadLock();
        }
    }

    public void SetCustomId(long key, long idProp)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();
                si.SetIndex(0, true, _customIdProperty.Count);
                _customIdProperty.Add(idProp);
                _customIdPropertyReverse.Add(key);

                _customIndexes.Add(key, si);
            }
            else
            {
                var indexProp = index.GetIndex(0);
                if (indexProp != -1)
                {
                    _customIdProperty[indexProp] = idProp;
                }
                else
                {
                    index.SetIndex(0, true, _customIdProperty.Count);
                    _customIdProperty.Add(idProp);
                    _customIdPropertyReverse.Add(key);
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }

    private readonly List<long> _customIntPropPropertyReverse = [];
    private readonly List<int> _customIntPropProperty = [];

    public bool TryGetCustomIntProp(long key, out int intProp)
    {
        _customSlimLock.EnterReadLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                intProp = 0;
                return false;
            }
            else
            {
                var listIndex = index.GetIndex(1);
                if (listIndex == -1)
                {
                    intProp = 0;
                    return false;
                }
                else
                {
                    intProp = _customIntPropProperty[listIndex];
                    return true;
                }
            }
        }
        finally
        {
            _customSlimLock.ExitReadLock();
        }
    }

    public void SetCustomIntProp(long key, int intProp)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();

                si.SetIndex(1, true, _customIntPropProperty.Count);
                _customIntPropProperty.Add(intProp);
                _customIntPropPropertyReverse.Add(key);

                _customIndexes.Add(key, si);
            }
            else
            {
                var indexProp = index.GetIndex(1);
                if (indexProp != -1)
                {
                    _customIntPropProperty[indexProp] = intProp;
                }
                else
                {
                    index.SetIndex(1, true, _customIntPropProperty.Count);
                    _customIntPropProperty.Add(intProp);
                    _customIntPropPropertyReverse.Add(key);
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }

    private readonly List<long> _customDoublePropPropertyReverse = [];
    private readonly List<double> _customDoublePropProperty = [];

    public bool TryGetCustomDoubleProp(long key, out double? doubleProp)
    {
        _customSlimLock.EnterReadLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                doubleProp = null;
                return false;
            }
            else
            {
                var listIndex = index.GetIndex(2);
                if (listIndex == -1)
                {
                    doubleProp = null;
                    return false;
                }
                else
                {
                    doubleProp = _customDoublePropProperty[listIndex];
                    return true;
                }
            }
        }
        finally
        {
            _customSlimLock.ExitReadLock();
        }
    }

    public void SetCustomDoubleProp(long key, double? doubleProp)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();

                var haveDoubleProp = doubleProp.HasValue;
                si.SetIndex(2, haveDoubleProp, _customDoublePropProperty.Count);
                if (haveDoubleProp)
                {
                    _customDoublePropProperty.Add(doubleProp.Value);
                    _customDoublePropPropertyReverse.Add(key);
                }

                _customIndexes.Add(key, si);
            }
            else
            {
                var haveDoubleProp = doubleProp.HasValue;
                var childOldIndex = index.GetIndex(2);
                if (haveDoubleProp)
                {
                    if (childOldIndex == -1)
                    {
                        index.SetIndex(2, true, _customDoublePropProperty.Count - 1);
                        _customDoublePropProperty.Add(doubleProp.Value);
                    }
                    else
                    {
                        index.SetIndex(2, true, childOldIndex);
                        _customDoublePropProperty[childOldIndex] = doubleProp.Value;
                    }
                }
                else if (childOldIndex != -1)
                {
                    index.SetIndex(2, false, childOldIndex);
                    if (childOldIndex == _customDoublePropProperty.Count - 1)
                    {
                        _customDoublePropProperty.RemoveAt(childOldIndex);
                        _customDoublePropPropertyReverse.RemoveAt(childOldIndex);
                    }
                    else
                    {
                        var endOfListIndex = _customDoublePropProperty.Count - 1;
                        var swapId = _customDoublePropPropertyReverse[endOfListIndex];
                        _customDoublePropPropertyReverse[childOldIndex] = swapId;
                        _customDoublePropPropertyReverse.RemoveAt(endOfListIndex);

                        ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                        swapIndex.SetIndex(2, false, childOldIndex);
                        _customDoublePropProperty[childOldIndex] = _customDoublePropProperty[endOfListIndex];
                        _customDoublePropProperty.RemoveAt(childOldIndex);
                    }
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }

    private readonly List<long> _customChildPropertyReverse = [];
    private readonly List<ValueClass> _customChildProperty = [];

    public bool TryGetCustomChildProp(long key, out ValueClass childProp)
    {
        _customSlimLock.EnterReadLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                childProp = null;
                return false;
            }
            else
            {
                var listIndex = index.GetIndex(3);
                if (listIndex == -1)
                {
                    childProp = null;
                    return false;
                }
                else
                {
                    childProp = _customChildProperty[listIndex];
                    return true;
                }
            }
        }
        finally
        {
            _customSlimLock.ExitReadLock();
        }
    }

    public void SetCustomChildProp(long key, ValueClass child)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();

                var haveChild = child != null;
                si.SetIndex(3, haveChild, _customChildProperty.Count);
                if (haveChild)
                {
                    _customChildProperty.Add(child);
                    _customChildPropertyReverse.Add(key);
                }

                _customIndexes.Add(key, si);
            }
            else
            {
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
                else if (childOldIndex != -1)
                {
                    index.SetIndex(3, false, childOldIndex);
                    if (childOldIndex == _customChildProperty.Count - 1)
                    {
                        _customChildProperty.RemoveAt(childOldIndex);
                        _customChildPropertyReverse.RemoveAt(childOldIndex);
                    }
                    else
                    {
                        var endOfListIndex = _customChildProperty.Count - 1;
                        var swapId = _customChildPropertyReverse[endOfListIndex];
                        _customChildPropertyReverse[childOldIndex] = swapId;
                        _customChildPropertyReverse.RemoveAt(endOfListIndex);

                        ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                        swapIndex.SetIndex(3, false, childOldIndex);
                        _customChildProperty[childOldIndex] = _customChildProperty[endOfListIndex];
                        _customChildProperty.RemoveAt(childOldIndex);
                    }
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }
}