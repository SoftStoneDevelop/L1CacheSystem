using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using L1CacheSystem.Tests.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Threading.Tasks;

namespace L1CacheSystem.Tests;

public partial interface ITestLockL1CacheSystem
{
}

[EntityCacheType(
        cacheType: typeof(TestClass),
        interfaceType: typeof(ITestLockL1CacheSystem),
        cacheName: "Entity",
        propAsKey: nameof(TestClass.Id),
        metricCollectFlags: MetricCollectBits.DefaultCollectBits,
        copyMethodType: CopyMethodType.CopyConstructor)]
[KeyColumnCache(
    keyType: typeof(long),
    interfaceType: typeof(ITestLockL1CacheSystem),
    cacheName: "Column"),
    ColumnCache(
    columnType: typeof(long),
    columnName: nameof(TestClass.Id)),
    ColumnCache(
    columnType: typeof(int),
    columnName: nameof(TestClass.IntProp)),
    ColumnCache(
    columnType: typeof(InnerTestClass),
    columnName: nameof(TestClass.InnerClass))]
public sealed partial class TestLockCacheSystem : L1CacheSystemBase, ITestLockL1CacheSystem
{
    public TestLockCacheSystem(
        ILogger logger,
        TimeProvider timeProvider,
        IMeterFactory meterFactory)
        : base(logger, timeProvider, meterFactory)
    {
    }
}

internal class LockGuardTests
{
    [Test]
    [CancelAfter(5_000)]
    public async Task GetLockEntityTest()
    {
        var factory = new FakeMeterFactory();
        using var cache = new TestLockCacheSystem(NullLogger.Instance, TimeProvider.System, factory);

        var lockGuard = new LockGuard(NullLogger.Instance, null);
        var lockGuard2 = new LockGuard(NullLogger.Instance, null);

        var getLockTask = cache.GetEntityLockAsync(1, lockGuard);
        Assert.That(getLockTask.IsCompleted, Is.True);
        Assert.That(lockGuard.GetTotalLockCount("lock:entity:1"), Is.EqualTo(1));

        var getLockTask2 = cache.GetEntityLockAsync(1, lockGuard2);
        Assert.That(getLockTask2.IsCompleted, Is.False);
        Assert.That(lockGuard2.GetTotalLockCount("lock:entity:1"), Is.EqualTo(0));

        lockGuard.Dispose();

        await getLockTask2;

        Assert.That(getLockTask2.IsCompleted, Is.True);
        Assert.That(lockGuard2.GetTotalLockCount("lock:entity:1"), Is.EqualTo(1));

        lockGuard2.Dispose();
    }

    [Test]
    [CancelAfter(5_000)]
    public async Task GetLockColumnTest()
    {
        var factory = new FakeMeterFactory();
        using var cache = new TestLockCacheSystem(NullLogger.Instance, TimeProvider.System, factory);

        var lockGuard = new LockGuard(NullLogger.Instance, null);
        var lockGuard2 = new LockGuard(NullLogger.Instance, null);

        var getLockTask = cache.GetColumnLockAsync(1, lockGuard);
        Assert.That(getLockTask.IsCompleted, Is.True);
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(1));

        var getLockTask2 = cache.GetColumnLockAsync(1, lockGuard2);
        Assert.That(getLockTask2.IsCompleted, Is.False);
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(0));

        lockGuard.Dispose();

        await getLockTask2;

        Assert.That(getLockTask2.IsCompleted, Is.True);
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(1));

        lockGuard2.Dispose();
    }

    [Test]
    [CancelAfter(5_000)]
    public async Task GetLockCancelTest()
    {
        var factory = new FakeMeterFactory();
        using var cache = new TestLockCacheSystem(NullLogger.Instance, TimeProvider.System, factory);

        var lockGuard = new LockGuard(NullLogger.Instance, null);
        var lockGuard2 = new LockGuard(NullLogger.Instance, null);

        var getLockTask = cache.GetColumnLockAsync(1, lockGuard);

        using var cts = new CancellationTokenSource();
        var getLockTask2 = cache.GetColumnLockAsync(1, lockGuard2, cts.Token);
        await cts.CancelAsync();
        await Assert.ThrowsAsync<OperationCanceledException>(async () => await getLockTask2);

        lockGuard.Dispose();
        lockGuard2.Dispose();
    }

    [Test]
    [CancelAfter(5_000)]
    public async Task GetLockReqursive()
    {
        var factory = new FakeMeterFactory();
        using var cache = new TestLockCacheSystem(NullLogger.Instance, TimeProvider.System, factory);

        var lockGuard = new LockGuard(NullLogger.Instance, null);

        await cache.GetColumnLockAsync(1, lockGuard);
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(1));
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(1));

        await cache.GetColumnLockAsync(1, lockGuard);
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(2));
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(2));

        var lockGuard2 = new LockGuard(NullLogger.Instance, lockGuard);

        await cache.GetColumnLockAsync(1, lockGuard2);
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(3));
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(3));
        Assert.That(lockGuard2.GetLockCount("lock:column:1"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(3));

        await cache.GetColumnLockAsync(1, lockGuard2);
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(4));
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(4));
        Assert.That(lockGuard2.GetLockCount("lock:column:1"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(4));

        await cache.GetColumnLockAsync(1, lockGuard2);
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard2.GetLockCount("lock:column:1"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));

        await cache.GetColumnLockAsync(2, lockGuard2);
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard2.GetLockCount("lock:column:1"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));

        Assert.That(lockGuard.GetLockCount("lock:column:2"), Is.EqualTo(0));
        // We know nothing about the descendants, since that is not the correct usage. Only the descendants know about their parents.
        Assert.That(lockGuard.GetTotalLockCount("lock:column:2"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetLockCount("lock:column:2"), Is.EqualTo(1));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:2"), Is.EqualTo(1));

        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard2.GetLockCount("lock:column:1"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));

        await cache.GetColumnLockAsync(2, lockGuard2);
        Assert.That(lockGuard.GetLockCount("lock:column:2"), Is.EqualTo(0));
        Assert.That(lockGuard.GetTotalLockCount("lock:column:2"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetLockCount("lock:column:2"), Is.EqualTo(2));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:2"), Is.EqualTo(2));

        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(5));
        Assert.That(lockGuard2.GetLockCount("lock:column:1"), Is.EqualTo(0));
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(5));

        lockGuard2.Dispose();

        Assert.That(lockGuard.GetLockCount("lock:column:1"), Is.EqualTo(2));
        Assert.That(lockGuard.GetTotalLockCount("lock:column:1"), Is.EqualTo(2));
        Assert.That(lockGuard.GetLockCount("lock:column:2"), Is.EqualTo(0));
        Assert.That(lockGuard.GetTotalLockCount("lock:column:2"), Is.EqualTo(0));

        lockGuard.Dispose();
    }

    [Test]
    [CancelAfter(5_000)]
    public async Task NotCorrectDisposeOrder()
    {
        var factory = new FakeMeterFactory();
        using var cache = new TestLockCacheSystem(NullLogger.Instance, TimeProvider.System, factory);

        var lockGuard = new LockGuard(NullLogger.Instance, null);

        await cache.GetColumnLockAsync(1, lockGuard);

        var lockGuard2 = new LockGuard(NullLogger.Instance, lockGuard);

        await cache.GetColumnLockAsync(1, lockGuard2);
        await cache.GetColumnLockAsync(1, lockGuard2);
        Assert.That(lockGuard2.GetTotalLockCount("lock:column:1"), Is.EqualTo(3));

        var execption = Assert.Throws<Exception>(async () => lockGuard.Dispose());
        Assert.That(execption.Message, Is.EqualTo($"Key 'lock:column:1': upon releasing {nameof(LockGuard)}, no locks should remain (lock count: 2)"));
    }
}