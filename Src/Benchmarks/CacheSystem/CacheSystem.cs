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
    interfaceType: typeof(ICacheSystem),
    columnName: nameof(ValueClass.Id)),
    ColumnCache(
    columnType: typeof(int),
    interfaceType: typeof(ICacheSystem),
    columnName: nameof(ValueClass.IntProp)),
    ColumnCache(
    columnType: typeof(double?),
    interfaceType: typeof(ICacheSystem),
    columnName: nameof(ValueClass.DoubleProp)),
    ColumnCache(
    columnType: typeof(ValueClass),
    interfaceType: typeof(ICacheSystem),
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

    private readonly Dictionary<long, SparseIndex> _customIndexes = [];

    private readonly ReaderWriterLockSlim _customSlimLock = new();
    private readonly List<long> _customIdPropertyReverse = [];
    private readonly List<long> _customIdProperty = [];

    public ref struct SetViewInfoRef<T>
    {
        public T Source { get; set; }

        public SetViewOperation IdProp { get; set; }
        public readonly Func<T, long>? FuncIdProp { get; init; }
        public readonly long GetIdProp() => FuncIdProp!(Source);

        public SetViewOperation IntProp { get; set; }
        public readonly Func<T, int>? FuncIntProp { get; init; }
        public readonly int GetIntProp() => FuncIntProp!(Source);

        public SetViewOperation DoubleProp { get; set; }
        public readonly Func<T, double?>? FuncDoubleProp { get; init; }
        public readonly double? GetDoubleProp() => FuncDoubleProp!(Source);

        public SetViewOperation ChildProp { get; set; }
        public readonly Func<T, ValueClass?>? FuncChildProp { get; init; }
        public readonly ValueClass? GetChildProp() => FuncChildProp!(Source);
    }

    public void SetCustomFromView<T>(long key, in SetViewInfoRef<T> viewInfo)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();
                if (viewInfo.IdProp == SetViewOperation.Set)
                {
                    si.SetIndex(0, true, _customIdProperty.Count);
                    _customIdProperty.Add(viewInfo.GetIdProp());
                    _customIdPropertyReverse.Add(key);
                }

                if (viewInfo.IntProp == SetViewOperation.Set)
                {
                    si.SetIndex(1, true, _customIntPropProperty.Count);
                    _customIntPropProperty.Add(viewInfo.GetIntProp());
                    _customIntPropPropertyReverse.Add(key);
                }

                if (viewInfo.DoubleProp == SetViewOperation.Set)
                {
                    si.SetIndex(2, true, _customDoublePropProperty.Count);
                    _customDoublePropProperty.Add(viewInfo.GetDoubleProp());
                    _customDoublePropPropertyReverse.Add(key);
                }

                if (viewInfo.ChildProp == SetViewOperation.Set)
                {
                    si.SetIndex(3, true, _customChildProperty.Count);
                    _customChildProperty.Add(viewInfo.GetChildProp());
                    _customChildPropertyReverse.Add(key);
                }

                _customIndexes.Add(key, si);
            }
            else
            {
                if (viewInfo.IdProp != SetViewOperation.None)
                {
                    var propIndex = index.GetIndex(0);
                    if (propIndex == -1)
                    {
                        if (viewInfo.IdProp == SetViewOperation.Set)
                        {
                            index.SetIndex(0, true, _customIdProperty.Count);
                            _customIdProperty.Add(viewInfo.GetIdProp());
                            _customIdPropertyReverse.Add(key);
                        }
                    }
                    else
                    {
                        if (viewInfo.IdProp == SetViewOperation.Set)
                        {
                            _customIdProperty[propIndex] = viewInfo.GetIdProp();
                        }
                        else
                        {
                            if (propIndex == _customIdProperty.Count - 1)
                            {
                                _customIdProperty.RemoveAt(propIndex);
                                _customIdPropertyReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _customIdProperty.Count - 1;
                                var swapId = _customIdPropertyReverse[endOfListIndex];
                                _customIdPropertyReverse[propIndex] = swapId;
                                _customIdPropertyReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                                swapIndex.SetIndex(0, false, propIndex);
                                _customIdProperty[propIndex] = _customIdProperty[endOfListIndex];
                                _customIdProperty.RemoveAt(endOfListIndex);
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
                            index.SetIndex(1, true, _customIntPropProperty.Count);
                            _customIntPropProperty.Add(viewInfo.GetIntProp());
                            _customIntPropPropertyReverse.Add(key);
                        }
                    }
                    else
                    {
                        if (viewInfo.IntProp == SetViewOperation.Set)
                        {
                            _customIntPropProperty[propIndex] = viewInfo.GetIntProp();
                        }
                        else
                        {
                            if (propIndex == _customIntPropProperty.Count - 1)
                            {
                                _customIntPropProperty.RemoveAt(propIndex);
                                _customIntPropPropertyReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _customIntPropProperty.Count - 1;
                                var swapId = _customIntPropPropertyReverse[endOfListIndex];
                                _customIntPropPropertyReverse[propIndex] = swapId;
                                _customIntPropPropertyReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                                swapIndex.SetIndex(1, false, propIndex);
                                _customIntPropProperty[propIndex] = _customIntPropProperty[endOfListIndex];
                                _customIntPropProperty.RemoveAt(endOfListIndex);
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
                            index.SetIndex(2, true, _customDoublePropProperty.Count);
                            _customDoublePropProperty.Add(viewInfo.GetDoubleProp());
                            _customDoublePropPropertyReverse.Add(key);
                        }
                    }
                    else
                    {
                        if (viewInfo.DoubleProp == SetViewOperation.Set)
                        {
                            _customDoublePropProperty[propIndex] = viewInfo.GetDoubleProp();
                        }
                        else
                        {
                            if (propIndex == _customDoublePropProperty.Count - 1)
                            {
                                _customDoublePropProperty.RemoveAt(propIndex);
                                _customIntPropPropertyReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _customDoublePropProperty.Count - 1;
                                var swapId = _customDoublePropPropertyReverse[endOfListIndex];
                                _customDoublePropPropertyReverse[propIndex] = swapId;
                                _customDoublePropPropertyReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                                swapIndex.SetIndex(2, false, propIndex);
                                _customDoublePropProperty[propIndex] = _customDoublePropProperty[endOfListIndex];
                                _customDoublePropProperty.RemoveAt(endOfListIndex);
                            }
                        }
                    }
                }

                if (viewInfo.ChildProp != SetViewOperation.None)
                {
                    var propIndex = index.GetIndex(3);
                    if (propIndex == -1)
                    {
                        if (viewInfo.ChildProp == SetViewOperation.Set)
                        {
                            index.SetIndex(3, true, _customChildProperty.Count);
                            _customChildProperty.Add(viewInfo.GetChildProp());
                            _customChildPropertyReverse.Add(key);
                        }
                    }
                    else
                    {
                        if (viewInfo.ChildProp == SetViewOperation.Set)
                        {
                            _customChildProperty[propIndex] = viewInfo.GetChildProp();
                        }
                        else
                        {
                            if (propIndex == _customDoublePropProperty.Count - 1)
                            {
                                _customChildProperty.RemoveAt(propIndex);
                                _customChildPropertyReverse.RemoveAt(propIndex);
                            }
                            else
                            {
                                var endOfListIndex = _customChildProperty.Count - 1;
                                var swapId = _customChildPropertyReverse[endOfListIndex];
                                _customChildPropertyReverse[propIndex] = swapId;
                                _customChildPropertyReverse.RemoveAt(endOfListIndex);

                                ref SparseIndex swapIndex = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, swapId);
                                swapIndex.SetIndex(3, false, propIndex);
                                _customChildPropertyReverse[propIndex] = _customChildPropertyReverse[endOfListIndex];
                                _customChildPropertyReverse.RemoveAt(endOfListIndex);
                            }
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
    private readonly List<double?> _customDoublePropProperty = [];

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

                si.SetIndex(2, true, _customDoublePropProperty.Count);
                _customDoublePropProperty.Add(doubleProp);
                _customDoublePropPropertyReverse.Add(key);

                _customIndexes.Add(key, si);
            }
            else
            {
                var indexProp = index.GetIndex(2);
                if (indexProp != -1)
                {
                    _customDoublePropProperty[indexProp] = doubleProp;
                }
                else
                {
                    index.SetIndex(2, true, _customDoublePropProperty.Count);
                    _customDoublePropProperty.Add(doubleProp);
                    _customDoublePropPropertyReverse.Add(key);
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }

    private readonly List<long> _customChildPropertyReverse = [];
    private readonly List<ValueClass?> _customChildProperty = [];

    public void ForeachByChildPropColumn(Action<long, ValueClass?> childPropAction)
    {
        _customSlimLock.EnterReadLock();
        try
        {
            var spanChildProp = CollectionsMarshal.AsSpan(_customChildProperty);
            var spanChildPropReverse = CollectionsMarshal.AsSpan(_customChildPropertyReverse);
            for (int i = 0; i < spanChildProp.Length; i++)
            {
                childPropAction(spanChildPropReverse[i], spanChildProp[i]);
            }
        }
        finally
        {
            _customSlimLock.ExitReadLock();
        }
    }

    public bool TryGetCustomChildProp(long key, out ValueClass? childProp)
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

    public void SetCustomChildProp(long key, ValueClass? child)
    {
        _customSlimLock.EnterWriteLock();
        try
        {
            ref SparseIndex index = ref CollectionsMarshal.GetValueRefOrNullRef(_customIndexes, key);
            if (System.Runtime.CompilerServices.Unsafe.IsNullRef(ref index))
            {
                var si = new SparseIndex();

                si.SetIndex(3, true, _customChildProperty.Count);
                _customChildProperty.Add(child);
                _customChildPropertyReverse.Add(key);

                _customIndexes.Add(key, si);
            }
            else
            {
                var indexProp = index.GetIndex(3);
                if (indexProp != -1)
                {
                    _customChildProperty[indexProp] = child;
                }
                else
                {
                    index.SetIndex(3, true, _customChildProperty.Count);
                    _customChildProperty.Add(child);
                    _customChildPropertyReverse.Add(key);
                }
            }
        }
        finally
        {
            _customSlimLock.ExitWriteLock();
        }
    }
}