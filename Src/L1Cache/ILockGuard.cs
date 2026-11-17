using L1CacheSystem.Helpers;
using System;

namespace L1CacheSystem;

public interface ILockGuard : IDisposable
{
    internal bool AddLock(SemaphoreSlimOwner lockOwner, string resource);

    /// <summary>
    /// Increment the total lock counter for a recursive resource lock
    /// </summary>
    internal bool IncrementCountIfContainLock(string resource);

    /// <summary>
    /// Reduce the number of locks in this specific instance
    /// </summary>
    internal void DecrementPairsLock(string resource);

    /// <summary>
    /// Release all acquired locks.
    /// Convenient to use when there is a single instance <see cref="ILockGuard"/> sequentially independent operations within a loop
    /// </summary>
    void ReleaseLocks();
}