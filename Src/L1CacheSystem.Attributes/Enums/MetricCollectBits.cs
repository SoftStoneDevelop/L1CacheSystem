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
    /// The time taken for the GetAll method call to execute
    /// </summary>
    GetAllCallTime = 1 << 1,

    /// <summary>
    /// Number of calls to the Get method
    /// </summary>
    GetCallsCount = 1 << 2,

    /// <summary>
    /// The time taken for the Get method call to execute
    /// </summary>
    GetCallTime = 1 << 3,

    /// <summary>
    /// Number of calls to the Set method
    /// </summary>
    SetCallsCount = 1 << 4,

    /// <summary>
    /// The time taken for the Set method call to execute
    /// </summary>
    SetCallTime = 1 << 5,

    /// <summary>
    /// Time taken to achieve a lock
    /// </summary>
    GetLockTime = 1 << 6,

    /// <summary>
    /// Number of elements in the cache
    /// </summary>
    ItemsCount = 1 << 7,

    /// <summary>
    /// Number of calls to the Delete method
    /// </summary>
    DeleteCallsCount = 1 << 8,

    /// <summary>
    /// The time taken for the Delete method call to execute
    /// </summary>
    DeleteCallTime = 1 << 9,

    /// <summary>
    /// A minimal set of metrics
    /// </summary>
    DefaultCollectBits = ItemsCount,

    /// <summary>
    /// All counter metrics
    /// </summary>
    AllCount = ItemsCount | SetCallsCount | GetCallsCount | GetAllCallsCount | DeleteCallsCount,

    /// <summary>
    /// All time-related metrics
    /// </summary>
    AllTimes = GetLockTime | SetCallTime | GetCallTime | GetAllCallTime | DeleteCallTime,

    AllBits = ~0
}