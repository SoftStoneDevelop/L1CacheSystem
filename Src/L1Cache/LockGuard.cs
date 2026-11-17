using L1CacheSystem.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace L1CacheSystem;

public partial class LockGuard : ILockGuard
{
    [LoggerMessage(Level = LogLevel.Critical, Message = "Error releasing lock {key}; it has already been deleted")]
    private partial void LogKeyAlreadyDeleted(string key);

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
    internal int GetTotalLockCount(string key)
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

    /// <summary>
    /// For testing purposes, obtain information about the block
    /// </summary>
    internal int GetLockCount(string key)
    {
        var current = this;
        var lockInfo = current._locks.Find(wh => wh.Resource == key);
        if (lockInfo != null)
        {
            return lockInfo.Count;
        }

        return 0;
    }

    bool ILockGuard.AddLock(SemaphoreSlimOwner lockOwner, string resource)
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

    bool ILockGuard.IncrementCountIfContainLock(string resource)
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

    void ILockGuard.DecrementPairsLock(string resource)
    {
        if (_lockPairs.TryGetValue(resource, out var count))
        {
            _lockPairs[resource] = count - 1;
        }
    }

    public void ReleaseLocks()
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
                // Since this is clearly an incorrect use of the library and an error on the part of the user, we immediately throw an exception.
                throw new Exception($"Key '{lockInfo.Resource}': upon releasing {nameof(LockGuard)}, no locks should remain (lock count: {lockInfo.Count})");
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
    }

    public void Dispose()
    {
        ReleaseLocks();
        GC.SuppressFinalize(this);
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