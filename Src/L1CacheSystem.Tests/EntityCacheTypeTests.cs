using L1CacheSystem.Attributes;
using L1CacheSystem.Attributes.Enums;
using L1CacheSystem.Tests.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics.Metrics;

namespace L1CacheSystem.Tests;

public partial interface IL1CacheSystem
{
}

[EntityCacheType(
        cacheType: typeof(TestClass),
        interfaceType: typeof(IL1CacheSystem),
        cacheName: "One",
        propAsKey: nameof(TestClass.Id),
        metricCollectFlags: MetricCollectBits.DefaultCollectBits,
        copyMethodType: CopyMethodType.CopyConstructor)]
[EntityCacheType(
        cacheType: typeof(TestClass),
        interfaceType: typeof(IL1CacheSystem),
        cacheName: "Two",
        propAsKey: nameof(TestClass.Id),
        metricCollectFlags: MetricCollectBits.None,
        copyMethodType: CopyMethodType.CopyConstructor)]
[EntityCacheType(
        cacheType: typeof(TestClass),
        interfaceType: typeof(IL1CacheSystem),
        cacheName: "Three",
        propAsKey: nameof(TestClass.Id),
        metricCollectFlags: MetricCollectBits.DefaultCollectBits,
        copyMethodName: nameof(TestClass.MySuperCopyMethod),
        copyMethodType: CopyMethodType.CopyMethodOnInstance)]
[EntityCacheType(
        cacheType: typeof(TestClass),
        interfaceType: typeof(IL1CacheSystem),
        cacheName: "Four",
        propAsKey: nameof(TestClass.Id),
        metricCollectFlags: MetricCollectBits.None,
        copyMethodName: nameof(TestClass.MySuperStaticCopyMethod),
        copyMethodType: CopyMethodType.CopyMethodOnType)]
public sealed partial class TestCacheSystem : L1CacheSystemBase, IL1CacheSystem
{
    public TestCacheSystem(
        ILogger logger,
        TimeProvider timeProvider,
        IMeterFactory meterFactory)
        : base(logger, timeProvider, meterFactory)
    {
    }
}

internal class EntityCacheTypeTests
{
    [Test]
    public void TestSetRead()
    {
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystem(NullLogger.Instance, TimeProvider.System, factory);
        IL1CacheSystem icache = cache;

        var original = new TestClass() { Id = 12 };
        cache.SetOne(original);
        cache.SetTwo(original);
        cache.SetThree(original);
        cache.SetFour(original);

        var one1 = cache.GetOne(12, Enums.ReturnType.Reference);
        var one2 = cache.GetOne(12, Enums.ReturnType.Reference);
        var ione1 = icache.GetOne(12, Enums.ReturnType.Reference);
        var ione2 = icache.GetOne(12, Enums.ReturnType.Reference);
        Assert.That(Is.ReferenceEquals(one1, one2), Is.True);
        Assert.That(Is.ReferenceEquals(ione1, ione2), Is.True);
        Assert.That(Is.ReferenceEquals(ione1, one2), Is.True);
        Assert.That(Is.ReferenceEquals(one1, original), Is.False);

        var two1 = cache.GetTwo(12, Enums.ReturnType.Reference);
        var two2 = cache.GetTwo(12, Enums.ReturnType.Reference);
        var itwo1 = icache.GetTwo(12, Enums.ReturnType.Reference);
        var itwo2 = icache.GetTwo(12, Enums.ReturnType.Reference);
        Assert.That(Is.ReferenceEquals(two1, two2), Is.True);
        Assert.That(Is.ReferenceEquals(itwo1, itwo2), Is.True);
        Assert.That(Is.ReferenceEquals(itwo1, two2), Is.True);
        Assert.That(Is.ReferenceEquals(two1, original), Is.False);

        var three1 = cache.GetThree(12, Enums.ReturnType.Reference);
        var three2 = cache.GetThree(12, Enums.ReturnType.Reference);
        var ithree1 = icache.GetThree(12, Enums.ReturnType.Reference);
        var ithree2 = icache.GetThree(12, Enums.ReturnType.Reference);
        Assert.That(Is.ReferenceEquals(three1, three2), Is.True);
        Assert.That(Is.ReferenceEquals(ithree1, ithree2), Is.True);
        Assert.That(Is.ReferenceEquals(ithree1, three2), Is.True);
        Assert.That(Is.ReferenceEquals(three1, original), Is.False);

        var four1 = cache.GetFour(12, Enums.ReturnType.Reference);
        var four2 = cache.GetFour(12, Enums.ReturnType.Reference);
        var ifour1 = icache.GetFour(12, Enums.ReturnType.Reference);
        var ifour2 = icache.GetFour(12, Enums.ReturnType.Reference);
        Assert.That(Is.ReferenceEquals(four1, four2), Is.True);
        Assert.That(Is.ReferenceEquals(ifour1, ifour2), Is.True);
        Assert.That(Is.ReferenceEquals(ifour1, four2), Is.True);
        Assert.That(Is.ReferenceEquals(four1, original), Is.False);

        Assert.That(Is.ReferenceEquals(one1, two1), Is.False);
        Assert.That(Is.ReferenceEquals(two1, three1), Is.False);
        Assert.That(Is.ReferenceEquals(three1, four1), Is.False);
    }

    [Test]
    public void TestSetReadCopy()
    {
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystem(NullLogger.Instance, TimeProvider.System, factory);
        IL1CacheSystem icache = cache;

        var original = new TestClass() { Id = 12 };
        cache.SetOne(original);
        cache.SetTwo(original);
        cache.SetThree(original);
        cache.SetFour(original);

        var one1 = cache.GetOne(12, Enums.ReturnType.Copy);
        var one2 = cache.GetOne(12, Enums.ReturnType.Copy);
        var ione1 = icache.GetOne(12, Enums.ReturnType.Copy);
        var ione2 = icache.GetOne(12, Enums.ReturnType.Copy);
        Assert.That(Is.ReferenceEquals(one1, one2), Is.False);
        Assert.That(Is.ReferenceEquals(ione1, ione2), Is.False);
        Assert.That(Is.ReferenceEquals(ione1, one2), Is.False);
        Assert.That(Is.ReferenceEquals(one1, original), Is.False);

        var two1 = cache.GetTwo(12, Enums.ReturnType.Copy);
        var two2 = cache.GetTwo(12, Enums.ReturnType.Copy);
        var itwo1 = icache.GetTwo(12, Enums.ReturnType.Copy);
        var itwo2 = icache.GetTwo(12, Enums.ReturnType.Copy);
        Assert.That(Is.ReferenceEquals(two1, two2), Is.False);
        Assert.That(Is.ReferenceEquals(itwo1, itwo2), Is.False);
        Assert.That(Is.ReferenceEquals(itwo1, two2), Is.False);
        Assert.That(Is.ReferenceEquals(two1, original), Is.False);

        var three1 = cache.GetThree(12, Enums.ReturnType.Copy);
        var three2 = cache.GetThree(12, Enums.ReturnType.Copy);
        var ithree1 = icache.GetThree(12, Enums.ReturnType.Copy);
        var ithree2 = icache.GetThree(12, Enums.ReturnType.Copy);
        Assert.That(Is.ReferenceEquals(three1, three2), Is.False);
        Assert.That(Is.ReferenceEquals(ithree1, ithree2), Is.False);
        Assert.That(Is.ReferenceEquals(ithree1, three2), Is.False);
        Assert.That(Is.ReferenceEquals(three1, original), Is.False);

        var four1 = cache.GetFour(12, Enums.ReturnType.Copy);
        var four2 = cache.GetFour(12, Enums.ReturnType.Copy);
        var ifour1 = icache.GetFour(12, Enums.ReturnType.Copy);
        var ifour2 = icache.GetFour(12, Enums.ReturnType.Copy);
        Assert.That(Is.ReferenceEquals(four1, four2), Is.False);
        Assert.That(Is.ReferenceEquals(ifour1, ifour2), Is.False);
        Assert.That(Is.ReferenceEquals(ifour1, four2), Is.False);
        Assert.That(Is.ReferenceEquals(four1, original), Is.False);

        Assert.That(Is.ReferenceEquals(one1, two1), Is.False);
        Assert.That(Is.ReferenceEquals(two1, three1), Is.False);
        Assert.That(Is.ReferenceEquals(three1, four1), Is.False);
    }

    [Test]
    public void TestClearCache()
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystem(NullLogger.Instance, timeProvider, factory);

        cache.SetOne(new TestClass() { Id = 12 }, expiry: TimeSpan.FromSeconds(1));

        var fromOneCache1 = cache.GetOne(12, Enums.ReturnType.Reference);
        Assert.That(fromOneCache1, Is.Not.Null);

        cache.ClearExpiredCache();

        fromOneCache1 = cache.GetOne(12, Enums.ReturnType.Reference);
        Assert.That(fromOneCache1, Is.Not.Null);

        timeProvider.FakeTime = timeProvider.FakeTime.AddSeconds(1);
        cache.ClearExpiredCache();

        fromOneCache1 = cache.GetOne(12, Enums.ReturnType.Reference);
        Assert.That(fromOneCache1, Is.Not.Null);

        timeProvider.FakeTime = timeProvider.FakeTime.AddSeconds(1);
        cache.ClearExpiredCache();

        fromOneCache1 = cache.GetOne(12, Enums.ReturnType.Reference);
        Assert.That(fromOneCache1, Is.Null);
    }
}