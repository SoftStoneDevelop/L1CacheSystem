using L1CacheSystem.Enums;
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
    private const string _columnName = "column_name";

    private readonly Meter _meter;
    private readonly string _serviceTypeName;

    private readonly KeyValuePair<string, object?> _instanceLabelTag;

    private readonly UpDownCounter<long> _itemsCount;
    private readonly UpDownCounter<long> _columnsCount;
    private readonly Histogram<double> _getLockTimeHistogram;

    private readonly Counter<long> _setCallsCount;
    private readonly Histogram<double> _setCallTime;

    private readonly Counter<long> _deleteCallsCount;
    private readonly Histogram<double> _deleteCallTime;

    private readonly Counter<long> _getCallsCount;
    private readonly Histogram<double> _getCallTime;

    private readonly Counter<long> _getAllCallsCount;
    private readonly Histogram<double> _getAllCallTime;

    private readonly Counter<long> _setColumnCallsCount;
    private readonly Histogram<double> _setColumnCallTime;

    private readonly Counter<long> _tryGetColumnCallsCount;
    private readonly Histogram<double> _tryGetColumnCallTime;

    private readonly Counter<long> _deleteColumnCallsCount;
    private readonly Histogram<double> _deleteColumnCallTime;

    private readonly Counter<long> _setFromViewCallsCount;
    private readonly Histogram<double> _setFromViewCallTime;

    private readonly Counter<long> _getFromViewCallsCount;
    private readonly Histogram<double> _getFromViewCallTime;

    public L1CacheSystemMetrics(IMeterFactory meterFactory, string serviceTypeName)
    {
        _serviceTypeName = serviceTypeName;
        _meter = meterFactory.Create(_meterName);
        _instanceLabelTag = new KeyValuePair<string, object?>(_instanceLabel, _serviceTypeName);

        _itemsCount = _meter.CreateUpDownCounter<long>(
            "item_count",
            description: "Total count in cache");

        _columnsCount = _meter.CreateUpDownCounter<long>(
            "columns_count",
            description: "Total count of columns in cache");

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

        _setColumnCallsCount = _meter.CreateCounter<long>("set_column_calls_count");
        _setColumnCallTime = _meter.CreateHistogram<double>(
            "set_column_call_time",
            unit: "us",
            description: "The time taken by the method");

        _tryGetColumnCallsCount = _meter.CreateCounter<long>("try_get_column_calls_count");
        _tryGetColumnCallTime = _meter.CreateHistogram<double>(
            "try_get_column_call_time",
            unit: "us",
            description: "The time taken by the method");

        _deleteColumnCallsCount = _meter.CreateCounter<long>("delete_column_calls_count");
        _deleteColumnCallTime = _meter.CreateHistogram<double>(
            "delete_column_call_time",
            unit: "us",
            description: "The time taken by the method");

        _setFromViewCallsCount = _meter.CreateCounter<long>("set_from_view_calls_count");
        _setFromViewCallTime = _meter.CreateHistogram<double>(
            "set_from_view_call_time",
            unit: "us",
            description: "The time taken by the method");

        _getFromViewCallsCount = _meter.CreateCounter<long>("get_from_view_calls_count");
        _getFromViewCallTime = _meter.CreateHistogram<double>(
            "get_from_view_call_time",
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
    /// Increment the item counter in the cache
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
    /// Decrease the column counter in the cache
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DecColumnsCount(string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabel, _serviceTypeName);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _columnsCount.Add(
            -1,
            tags);
    }

    /// <summary>
    /// Increment the column counter in the cache
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void IncColumnsCount(string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _columnsCount.Add(
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetColumnCallInc(string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _setColumnCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetColumnCallTime(double microseconds, string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _setColumnCallTime.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TryGetColumnCallInc(string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _tryGetColumnCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TryGetColumnCallTime(double microseconds, string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _tryGetColumnCallTime.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeleteColumnCallInc(string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _deleteColumnCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DeleteColumnCallTime(double microseconds, string cacheTypeName, string columnName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        tags.Add(_columnName, columnName);
        _deleteColumnCallTime.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFromViewCallInc(string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _setFromViewCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetFromViewCallTime(double microseconds, string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _setFromViewCallTime.Record(
            microseconds,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetFromViewCallInc(string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _getFromViewCallsCount.Add(
            1,
            tags);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GetFromViewCallTime(double microseconds, string cacheTypeName)
    {
        TagList tags = default;
        tags.Add(_instanceLabelTag);
        tags.Add(_cacheType, cacheTypeName);
        _getFromViewCallTime.Record(
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