using System;

namespace L1CacheSystem.Attributes.Enums;

/// <summary>
/// Flags specifying which metrics the system should collect
/// </summary>
[Flags]
public enum MetricCollectBits : int
{
    None = 0,

    /// <summary>
    /// Number of calls to the GetAll method
    /// </summary>
    GetAllCallsCount = 1,

    /// <summary>
    /// Number of calls to the ForeachColumn method
    /// </summary>
    ForeachColumnCallsCount = GetAllCallsCount,

    /// <summary>
    /// The time taken for the GetAll method call to execute
    /// </summary>
    GetAllCallTime = 1 << 1,

    /// <summary>
    /// The time taken for the ForeachColumn method call to execute
    /// </summary>
    ForeachColumnCallTime = GetAllCallTime,

    /// <summary>
    /// Number of calls to the Get method
    /// </summary>
    GetCallsCount = 1 << 2,

    /// <summary>
    /// Number of calls to the TryGet method
    /// </summary>
    TryGetCallsCount = GetCallsCount,

    /// <summary>
    /// The time taken for the Get method call to execute
    /// </summary>
    GetCallTime = 1 << 3,

    /// <summary>
    /// The time taken for the TryGet method call to execute
    /// </summary>
    TryGetCallTime = GetCallTime,

    /// <summary>
    /// Number of calls to the Set method
    /// </summary>
    SetCallsCount = 1 << 4,

    /// <summary>
    /// Number of calls to the SetColumn method
    /// </summary>
    SetColumnCallsCount = SetCallsCount,

    /// <summary>
    /// The time taken for the Set method call to execute
    /// </summary>
    SetCallTime = 1 << 5,

    /// <summary>
    /// The time taken for the Set Column method call to execute
    /// </summary>
    SetColumnCallTime = SetCallTime,

    /// <summary>
    /// Time taken to achieve a lock
    /// </summary>
    GetLockTime = 1 << 6,

    /// <summary>
    /// Number of elements in the cache
    /// </summary>
    ItemsCount = 1 << 7,

    /// <summary>
    /// Number of columns in the cache
    /// </summary>
    ColumnsCount = ItemsCount,

    /// <summary>
    /// Number of calls to the Delete method
    /// </summary>
    DeleteCallsCount = 1 << 8,

    /// <summary>
    /// The time taken for the Delete method call to execute
    /// </summary>
    DeleteCallTime = 1 << 9,

    /// <summary>
    /// Number of calls to the SetFromView method
    /// </summary>
    SetFromViewCallsCount = 1 << 10,

    /// <summary>
    /// The time taken for the SetFromView method call to execute
    /// </summary>
    SetFromViewCallTime = 1 << 11,

    /// <summary>
    /// Number of calls to the GetFromView method
    /// </summary>
    GetFromViewCallsCount = 1 << 12,

    /// <summary>
    /// The time taken for the GetFromView method call to execute
    /// </summary>
    GetFromViewCallTime = 1 << 13,

    /// <summary>
    /// Number of calls to the DeleteColumn method
    /// </summary>
    DeleteColumnCallsCount = 1 << 14,

    /// <summary>
    /// The time taken for the DeleteColumn method call to execute
    /// </summary>
    DeleteColumnCallTime = 1 << 15,

    /// <summary>
    /// A minimal set of metrics
    /// </summary>
    DefaultCollectBits = ItemsCount | ColumnsCount,

    /// <summary>
    /// All counter metrics
    /// </summary>
    AllCount =
        ItemsCount | ColumnsCount |
        SetCallsCount | SetColumnCallsCount |
        GetCallsCount | GetAllCallsCount |
        DeleteCallsCount |
        TryGetCallsCount |
        ForeachColumnCallsCount |
        SetFromViewCallsCount |
        GetFromViewCallsCount |
        DeleteColumnCallsCount,

    /// <summary>
    /// All time-related metrics
    /// </summary>
    AllTimes =
        GetLockTime |
        SetCallTime |SetColumnCallTime |
        GetCallTime | GetAllCallTime |
        DeleteCallTime |
        TryGetCallTime |
        ForeachColumnCallTime |
        SetFromViewCallTime |
        GetFromViewCallTime |
        DeleteColumnCallTime,

    AllBits = ~0
}