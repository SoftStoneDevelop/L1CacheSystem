using System;

namespace L1CacheSystem.Generator.Enums
{
    [Flags]
    public enum MetricCollectBits
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
        /// Number of calls to the SetFromView method
        /// </summary>
        SetFromViewCallsCount = 1 << 10,

        /// <summary>
        /// The time taken for the SetFromView method call to execute
        /// </summary>
        SetFromViewCallTime = 1 << 11,

        /// <summary>
        /// Number of calls to the GetIntoView method
        /// </summary>
        GetIntoViewCallsCount = 1 << 12,

        /// <summary>
        /// The time taken for the GetIntoView method call to execute
        /// </summary>
        GetIntoViewCallTime = 1 << 13
    }
}