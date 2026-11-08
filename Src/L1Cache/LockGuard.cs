using L1CacheSystem.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace L1CacheSystem;

public partial class LockGuard : ILockGuard
{
    [LoggerMessage(Level = LogLevel.Critical, Message = "Error releasing lock {key}; it has already been deleted")]
    private partial void LogKeyAlreadyDeleted(string key);

    [LoggerMessage(Level = LogLevel.Critical, Message = "{key}: upon releasing {lockGuard}, no locks should remain (lock count: {lockCount})")]
    private partial void LogHaveRemainLock(string key, string lockGuard, int lockCount);

    private readonly ILogger _logger;
    private readonly LockGuard? _parent;

    private LockGuard()
    {
        throw new NotImplementedException();
    }

    public LockGuard(
        ILogger logger,
        LockGuard? parent)
    {
        _logger = logger;
        _parent = parent;
    }

    private readonly List<InnerLockInfo> _locks = [];

    /// <summary>
    /// Number of locks in this instance (key, number of locks)
    /// </summary>
    private readonly Dictionary<string, int> _lockPairs = [];

    /// <summary>
    /// For testing purposes, obtain information about the block
    /// </summary>
    public int GetLockCount(string key)
    {
        var current = this;
        while (current != null)
        {
            var lockInfo = current._locks.Find(wh => wh.Resource == key);
            if (lockInfo != null)
            {
                return lockInfo.Count;
            }

            current = current._parent;
        }

        return 0;
    }

    public bool AddLock(SemaphoreSlimOwner lockOwner, string resource)
    {
        if (_lockPairs.TryGetValue(resource, out var count))
        {
            _lockPairs[resource] = count + 1;
        }
        else
        {
            _lockPairs.Add(resource, 1);
        }

        // Let’s find the source that holds the lock and increment the counter.
        var current = _parent;
        var lockInParent = false;
        while (current != null)
        {
            var lockInfo = current._locks.Find(wh => wh.Resource == resource);
            if (lockInfo != null)
            {
                lockInfo.Count++;
                lockInParent = true;
                break;
            }

            current = current._parent;
        }

        if (!lockInParent)
        {
            // We control the blocking
            _locks.Add(new InnerLockInfo(resource, lockOwner));
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IncrementCountIfContainLock(string resource)
    {
        var current = this;
        var containLock = false;
        while (current != null)
        {
            var lockInfo = current._locks.Find(wh => wh.Resource == resource);
            if (lockInfo != null)
            {
                lockInfo.Count++;
                containLock = true;
            }

            current = current._parent;
        }

        if (containLock)
        {
            if (_lockPairs.TryGetValue(resource, out var count))
            {
                _lockPairs[resource] = count + 1;
            }
            else
            {
                _lockPairs.Add(resource, 1);
            }
        }

        return containLock;
    }

    public void DecrementPairsLock(string resource)
    {
        if (_lockPairs.TryGetValue(resource, out var count))
        {
            _lockPairs[resource] = count - 1;
        }
    }

    public ValueTask ReleaseLocksAsync()
    {
        // Let’s decrement the resource recursive lock counter.
        var current = this;
        while (current != null)
        {
            foreach (var lockPair in _lockPairs)
            {
                var lockInfo = current._locks.Find(wh => wh.Resource == lockPair.Key);
                if (lockInfo != null)
                {
                    lockInfo.Count -= lockPair.Value;
                }
            }

            current = current._parent;
        }

        _lockPairs.Clear();
        var locksCount = _locks.Count;
        for (int i = locksCount - 1; i >= 0; i--)
        {
            var lockInfo = _locks[i];
            if (lockInfo.Count > 0)
            {
                LogHaveRemainLock(lockInfo.Resource, nameof(LockGuard), lockInfo.Count);
            }

            _locks.RemoveAt(i);
            try
            {
                lockInfo.LockOwner.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing lock {resource}", lockInfo.Resource);
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ReleaseLocksAsync();
    }

    sealed private class InnerLockInfo
    {
#pragma warning disable IDE0290 // Use primary constructor
        public InnerLockInfo(
#pragma warning restore IDE0290 // Use primary constructor
            string resource,
            SemaphoreSlimOwner lockOwner)
        {
            Count = 1;
            LockOwner = lockOwner;
            Resource = resource;
        }

        /// <summary>
        /// Total number of locks
        /// </summary>
        public int Count { get; set; }

        public string Resource { get; set; }

        public SemaphoreSlimOwner LockOwner { get; init; }
    }
}