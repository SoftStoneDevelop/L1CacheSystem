using L1CacheSystem.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace L1CacheSystem.Metrics;

public sealed class L1CacheSystemMetrics
{
    private const string _meterName = "l1_cache_system";
    private const string _instanceLabel = "l1_instance";
    private const string _cacheType = "cache_type";
    private const string _returnType = "return_type";
    private const string _valueOwnership = "value_ownership";

    private readonly Meter _meter;
    private readonly string _serviceTypeName;

    private readonly KeyValuePair<string, object?> _instanceLabelTag;

    private readonly UpDownCounter<long> _itemsCount;
    private readonly Histogram<double> _getLockTimeHistogram;

    private readonly Counter<long> _setCallsCount;
    private readonly Histogram<double> _setCallTime;

    private readonly Counter<long> _deleteCallsCount;
    private readonly Histogram<double> _deleteCallTime;

    private readonly Counter<long> _getCallsCount;
    private readonly Histogram<double> _getCallTime;

    private readonly Counter<long> _getAllCallsCount;
    private readonly Histogram<double> _getAllCallTime;

    public L1CacheSystemMetrics(IMeterFactory meterFactory, string serviceTypeName)
    {
        _serviceTypeName = serviceTypeName;
        _meter = meterFactory.Create(_meterName);
        _instanceLabelTag = new KeyValuePair<string, object?>(_instanceLabel, _serviceTypeName);

        _itemsCount = _meter.CreateUpDownCounter<long>(
            "item_count",
            description: "Total count in cache");

        _getLockTimeHistogram = _meter.CreateHistogram<double>(
            "get_lock_time",
            unit: "us",
            description: "The time taken by the method");

        _setCallsCount = _meter.CreateCounter<long>("set_calls_count");
        _setCallTime = _meter.CreateHistogram<double>(
            "set_call_time",
            unit: "us",
            description: "The time taken by the method");

        _deleteCallsCount = _meter.CreateCounter<long>("delete_calls_count");
        _deleteCallTime = _meter.CreateHistogram<double>(
            "delete_call_time",
            unit: "us",
            description: "The time taken by the method");

        _getCallsCount = _meter.CreateCounter<long>("get_calls_count");
        _getCallTime = _meter.CreateHistogram<double>(
            "get_call_time",
            unit: "us",
            description: "The time taken by the method");

        _getAllCallsCount = _meter.CreateCounter<long>("get_all_calls_count");
        _getAllCallTime = _meter.CreateHistogram<double>(
            "get_all_call_time",
            unit: "us",
            description: "The time taken by the method");
    }

    /// <summary>
    /// Decrease the item counter in the cache
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DecItemsCount(string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabel, _serviceTypeName);
        tags.Add(_cacheType, cacheTypeName);

        _itemsCount.Add(
            -1,
            tags);
    }

    /// <summary>
    /// Increment the element counter in the cache
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncItemsCount(string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _itemsCount.Add(
            1,
            tags);
    }

    /// <summary>
    /// Set a metric for the time taken to acquire a lock on a resource
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetLockObserve(double microseconds, string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _getLockTimeHistogram.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCallInc(string cacheTypeName, ValueOwnership valueOwnership)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_valueOwnership, ToStringLiteral(valueOwnership));
        _setCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetCallTime(double microseconds, string cacheTypeName, ValueOwnership valueOwnership)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_valueOwnership, ToStringLiteral(valueOwnership));
        _setCallTime.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeleteCallInc(string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _deleteCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeleteCallTime(double microseconds, string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _deleteCallTime.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetCallInc(string cacheTypeName, ReturnType returnType)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_returnType, ToStringLiteral(returnType));
        _getCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetCallTime(double microseconds, string cacheTypeName, ReturnType returnType)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_returnType, ToStringLiteral(returnType));
        _getCallTime.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllCallInc(string cacheTypeName, ReturnType returnType)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_returnType, ToStringLiteral(returnType));
        _getAllCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetAllCallTime(double microseconds, string cacheTypeName, ReturnType returnType)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_returnType, ToStringLiteral(returnType));
        _getAllCallTime.Record(
            microseconds,
            tags);
    }

    private static string ToStringLiteral(ReturnType returnType)
    {
        switch (returnType)
        {
            case ReturnType.Copy:
            {
                return "Copy";
            }

            case ReturnType.Reference:
            {
                return "Reference";
            }

            default:
            {
                return "Unknown";
            }
        }
    }

    private static string ToStringLiteral(ValueOwnership valueOwnership)
    {
        switch (valueOwnership)
        {
            case ValueOwnership.NotTransferred:
            {
                return "NotTransferred";
            }

            case ValueOwnership.Transferred:
            {
                return "Transferred";
            }

            default:
            {
                return "Unknown";
            }
        }
    }
}