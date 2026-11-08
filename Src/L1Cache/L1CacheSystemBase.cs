using L1CacheSystem.Helpers;
using L1CacheSystem.Metrics;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace L1CacheSystem;

public abstract partial class L1CacheSystemBase : IDisposable
{
    [LoggerMessage(Level = LogLevel.Trace, Message = "Clearing locks: Waiting for a resource lock {lockKey} - {inWait}")]
    private partial void LogClearingLocksWait(string lockKey, int inWait);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Error deleting an unused lock {lockKey}, a lock held by another party has been removed")]
    private partial void LogLockHeldByAnotherRemoved(string lockKey);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Error deleting unused lock {lockKey}; the lock is not present in the collection")]
    private partial void LogLockIsNotPresent(string lockKey);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Error deleting unused lock {lockKey}; the operation should always succeed")]
    private partial void LogLockExitBusyError(string lockKey);

    [LoggerMessage(Level = LogLevel.Critical, Message = "Error deleting expired value {resource}; the operation should always succeed")]
    private partial void LogResourceExitBusyError(string resource);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Waiting for the resource {resource} to be locked - {inWait}")]
    private partial void LogWaitingForResource(string resource, int inWait);

    [LoggerMessage(Level = LogLevel.Trace, Message = "Value not found {resource}")]
    private partial void LogValueNotFound(string resource);

    protected readonly ILogger _logger;
    protected readonly TimeProvider _timeProvider;
    protected readonly L1CacheSystemMetrics _metrics;

    private readonly ConcurrentDictionary<string, CacheLockInfo> _locks = new();

    private CancellationTokenSource? _cts;
    private Task? _removeExpireRoutine;

    public L1CacheSystemBase(
        ILogger logger,
        TimeProvider timeProvider,
        IMeterFactory meterFactory)
    {
        _logger = logger;
        _timeProvider = timeProvider;
        _metrics = new L1CacheSystemMetrics(meterFactory, GetType().Name);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _removeExpireRoutine = Task.Factory.StartNew(
            async () => await RemoveExpireRoutine(_cts.Token),
            _cts.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_removeExpireRoutine != null)
        {
            await TaskHelpers.WaitEndRoutine(_removeExpireRoutine, cancellationToken).ConfigureAwait(false);
            _removeExpireRoutine = null;
        }
    }

    /// <summary>
    /// The cleanup of outdated cache entries 
    /// </summary>
    protected virtual void RemoveExpiredUser()
    {
    }

    /// <summary>
    /// The cleanup of outdated cache entries 
    /// </summary>
    protected virtual void RemoveExpiredGenerated()
    {
    }

    private async Task RemoveExpireRoutine(CancellationToken token)
    {
        var delayTime = TimeSpan.FromSeconds(10);
        while (!token.IsCancellationRequested)
        {
            ClearExpiredCache();
            await Task.Delay(delayTime, token).ContinueWith((_) => { }, CancellationToken.None);
        }
    }

    internal void ClearExpiredCache()
    {
        try
        {
            RemoveExpiredUser();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in the cleanup loop for expired values");
        }

        try
        {
            RemoveExpiredGenerated();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in the cleanup loop for expired entries in generated caches");
        }

        try
        {
            var lockKeys = _locks.Keys.ToArray();
            foreach (var lockKey in lockKeys)
            {
                if (!_locks.TryGetValue(lockKey, out var cacheLock))
                {
                    continue;
                }

                var timeFromLastGetLock = _timeProvider.GetUtcNow() - cacheLock.LastGetLock;
                if (timeFromLastGetLock < TimeSpan.FromMinutes(2))
                {
                    continue;
                }

                if (!cacheLock.TryEnterBusy())
                {
                    continue;
                }

                var needDispose = false;
                try
                {
                    var inWait = cacheLock.InWait();
                    if (inWait != 0)
                    {
                        LogClearingLocksWait(lockKey, inWait);
                        continue;
                    }

                    if (_locks.TryRemove(lockKey, out var removedLock))
                    {
                        if (cacheLock != removedLock)
                        {
                            _locks.TryAdd(lockKey, removedLock);
                            LogLockHeldByAnotherRemoved(lockKey);
                        }
                        else
                        {
                            needDispose = true;
                        }
                    }
                    else
                    {
                        LogLockIsNotPresent(lockKey);
                    }
                }
                finally
                {
                    if (!cacheLock.TryExitBusy())
                    {
                        LogLockExitBusyError(lockKey);
                    }

                    if (needDispose)
                    {
                        cacheLock.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while clearing unused locks");
        }
    }

    protected void RemoveExpired<K, T>(
        ConcurrentDictionary<K, CacheValue<T>> storage,
        Func<K, string> getResource,
        string cacheTypeName,
        bool collectItemsCountMetric = false)
        where K : notnull
    {
        RemoveExpired(storage, _locks, getResource, cacheTypeName, collectItemsCountMetric);
    }

    private void RemoveExpired<K, T>(
        ConcurrentDictionary<K, CacheValue<T>> storage,
        ConcurrentDictionary<string, CacheLockInfo> locks,
        Func<K, string> getResource,
        string cacheTypeName,
        bool collectItemsCountMetric = false)
        where K : notnull
    {
        var keys = storage.Keys.ToArray();
        foreach (var key in keys)
        {
            string resource;
            if (!storage.TryGetValue(key, out var cacheValue))
            {
                resource = getResource(key);
                LogValueNotFound(resource);
                continue;
            }

            if (!cacheValue.Expire.HasValue)
            {
                continue;
            }

            resource = getResource(key);
            locks.TryGetValue(resource, out var keyLock);
            if (keyLock?.TryEnterBusy() == false)
            {
                continue;
            }

            try
            {
                var inWait = keyLock?.InWait() ?? 0;
                if (inWait != 0)
                {
                    LogWaitingForResource(resource, inWait);
                    continue;
                }

                if (_timeProvider.GetUtcNow() > cacheValue.Expire)
                {
                    if (storage.TryRemove(key, out var removedCache))
                    {
                        if (collectItemsCountMetric)
                        {
                            _metrics.DecItemsCount(cacheTypeName);
                        }

                        if (!ReferenceEquals(removedCache, cacheValue) && storage.TryAdd(key, removedCache))
                        {
                            // Restored what had been mistakenly deleted to the cache
                            if (collectItemsCountMetric)
                            {
                                _metrics.IncItemsCount(cacheTypeName);
                            }
                        }
                    }
                }
            }
            finally
            {
                if (keyLock?.TryExitBusy() == false)
                {
                    LogResourceExitBusyError(resource);
                }
            }
        }
    }

    private async Task<SemaphoreSlimOwner> GetLockAsync(
        string resource,
        string cacheTypeName,
        bool collectGetLockTimeMetric = false,
        CancellationToken cancellationToken = default)
    {
        long startGetLock = 0;
        if (collectGetLockTimeMetric)
        {
            startGetLock = _timeProvider.GetTimestamp();
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CacheLockInfo? resourceLock;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (_locks.TryGetValue(resource, out resourceLock))
                {
                    break;
                }

                var newLock = new CacheLockInfo(resource, _logger, _timeProvider);
                if (!_locks.TryAdd(resource, newLock))
                {
                    newLock.Dispose();
                    continue;
                }

                resourceLock = newLock;
                break;
            }

            var (Lock, Result) = await resourceLock.GetLockAsync(cancellationToken);
            switch (Result)
            {
                case GetLockResult.Locked:
                {
                    if (collectGetLockTimeMetric)
                    {
                        var ellapsed = _timeProvider.GetElapsedTime(startGetLock);
                        _metrics.GetLockObserve(ellapsed.TotalMicroseconds, cacheTypeName);
                    }

                    return Lock!;
                }

                case GetLockResult.TryAgain:
                {
                    continue;
                }

                case GetLockResult.NotLocked:
                case GetLockResult.Unknown:
                default:
                {
                    throw new Exception($"Failed to apply a lock '{resource}'");
                }
            }
        }
    }

    protected async Task GetLockAsync(
        string resource,
        string chacheTypeName,
        ILockGuard lockGuard,
        bool collectGetLockTimeMetric = false,
        CancellationToken cancellationToken = default)
    {
        // If it’s already blocked, we’ll increment the counter.
        if (!lockGuard.IncrementCountIfContainLock(resource))
        {
            try
            {
                // If a lock wasn't acquired within the session, we will acquire it.
                var lockInfo = await GetLockAsync(resource, chacheTypeName, collectGetLockTimeMetric, cancellationToken).ConfigureAwait(false);
                if (!lockGuard.AddLock(lockInfo, resource))
                {
                    lockGuard.DecrementPairsLock(resource);
                    lockInfo.Dispose();
                }
            }
            catch
            {
                lockGuard.DecrementPairsLock(resource);
                throw;
            }
        }
    }

    sealed protected class CacheValue<T>
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        private CacheValue()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        {
        }

        public CacheValue(T value, DateTimeOffset? expire = null)
        {
            Value = value;
            Expire = expire;
        }

        /// <summary>
        /// The date after which the value will be removed from the cache
        /// </summary>
        public DateTimeOffset? Expire { get; set; }

        public T Value { get; set; }
    }

    protected virtual void DisposeUser()
    {
    }

    protected virtual void DisposeGenerated()
    {
    }

    public void Dispose()
    {
        foreach (var keyLock in _locks)
        {
            keyLock.Value.Dispose();
        }

        _locks.Clear();

        try
        {
            DisposeUser();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling {methodName}", nameof(DisposeUser));
        }

        try
        {
            DisposeGenerated();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling {methodName}", nameof(DisposeGenerated));
        }
    }

    protected enum GetLockResult
    {
        Unknown = 0,

        NotLocked = 1,

        Locked = 2,

        TryAgain = 3,
    }

    protected partial class CacheLockInfo : IDisposable
    {
        [LoggerMessage(Level = LogLevel.Warning, Message = "Error while attempting to acquire resource {resource}; the operation must always succeed.")]
        private partial void LogAcquireTryExitBusy(string resource);

        [LoggerMessage(Level = LogLevel.Error, Message = "Wait-loop error while attempting to acquire resource {resource}.")]
        private partial void LogWaitLoopError(Exception ex, string resource);

        private readonly SemaphoreSlim _semaphore = new(1, 1);
        private readonly TimeProvider _timeProvider;
        private readonly string _resource;
        private readonly ILogger _logger;

        public CacheLockInfo(
            string resource,
            ILogger logger,
            TimeProvider timeProvider)
        {
            _resource = resource;
            _logger = logger;
            _timeProvider = timeProvider;
            LastGetLock = _timeProvider.GetUtcNow();
        }

        public DateTimeOffset LastGetLock { get; private set; }

        public async ValueTask<(SemaphoreSlimOwner? Lock, GetLockResult Result)> GetLockAsync(CancellationToken cancellationToken = default)
        {
            LastGetLock = _timeProvider.GetUtcNow();
            if (!TryEnterBusy())
            {
                return
                    (null, GetLockResult.TryAgain);
            }

            Interlocked.Increment(ref _inWait);

            if (!TryExitBusy())
            {
                LogAcquireTryExitBusy(_resource);
            }

            var startWait = _timeProvider.GetTimestamp();
            var getLockTimeSpan = TimeSpan.FromSeconds(10);
            var maxWaitTime = TimeSpan.FromMinutes(1);

            try
            {
                while (true)
                {
                    var getLock = _semaphore.Wait(0, cancellationToken);
                    if (getLock)
                    {
                        break;
                    }

                    getLock = await _semaphore.WaitAsync(getLockTimeSpan, cancellationToken).ConfigureAwait(false);
                    if (getLock)
                    {
                        break;
                    }

                    var elapsed = _timeProvider.GetElapsedTime(startWait);
                    if (elapsed > maxWaitTime)
                    {
                        _logger.LogWarning(
                            "The attempt to acquire resource {resource} has lasted longer than {elapsed}, which exceeds the maximum allowed wait time {maxWaitTime}",
                            _resource,
                            elapsed,
                            maxWaitTime);

                        Interlocked.Decrement(ref _inWait);
                        return
                            (null, GetLockResult.NotLocked);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "The attempt to acquire resource {resource} has been ongoing for more than {elapsed} ({getLockTimeSpan})",
                            _resource,
                            elapsed,
                            getLockTimeSpan);
                    }
                }
            }
            catch (Exception ex)
            {
                Interlocked.Decrement(ref _inWait);
                if (ex is not OperationCanceledException)
                {
                    LogWaitLoopError(ex, _resource);
                }

                return
                    (null, GetLockResult.NotLocked);
            }

            return
                (new SemaphoreSlimOwner(_semaphore, true, () => Interlocked.Decrement(ref _inWait)), GetLockResult.Locked);
        }

        public int InWait()
        {
            return _inWait;
        }

        private volatile int _inWait = 0;

        public bool TryEnterBusy()
        {
            return
                Interlocked.CompareExchange(ref _busy, 1, 0) == 0;
        }

        public bool TryExitBusy()
        {
            return
                Interlocked.CompareExchange(ref _busy, 0, 1) == 1;
        }

        private volatile int _busy = 0;

        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}