using L1CacheSystem.Attributes;
using L1CacheSystem.Tests.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics.Metrics;

namespace L1CacheSystem.Tests;

public partial interface IL1CacheSystemColumn
{
}

[KeyColumnCache(
    keyType: typeof(long),
    interfaceType: typeof(IL1CacheSystemColumn),
    cacheName: "One"),
    ColumnCache(
    columnType: typeof(long),
    columnName: nameof(TestClass.Id)),
    ColumnCache(
    columnType: typeof(int),
    columnName: nameof(TestClass.IntProp)),
    ColumnCache(
    columnType: typeof(InnerTestClass),
    columnName: nameof(TestClass.InnerClass))]
public sealed partial class TestCacheSystemColumn : L1CacheSystemBase, IL1CacheSystemColumn
{
    public TestCacheSystemColumn(
        ILogger logger,
        TimeProvider timeProvider,
        IMeterFactory meterFactory)
        : base(logger, timeProvider, meterFactory)
    {
    }
}

internal class ColumnCacheTypeTests
{
    [Test]
    public void TestSetRead()
    {
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystemColumn(NullLogger.Instance, TimeProvider.System, factory);
        IL1CacheSystemColumn icache = cache;

        var original = new TestClass()
        {
            Id = 12,
            IntProp = 19,
            InnerClass = new InnerTestClass()
            {
                IntProp = 44
            }
        };

        var view = new TestCacheSystemColumn.SetViewOne<TestClass>()
        {
            Id = Enums.SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            IntProp = Enums.SetViewOperation.Set,
            FuncIntProp = static (source) => source.IntProp,
            InnerClass = Enums.SetViewOperation.Set,
            FuncInnerClass = static (source) => source.InnerClass!
        };

        view.Source = original;

        cache.SetOneFromView(original.Id, view);

        var haveValue = cache.TryGetOneId(original.Id, out var id);
        Assert.That(haveValue, Is.True);
        Assert.That(id, Is.EqualTo(original.Id));
        haveValue = cache.TryGetOneId(99, out _);
        Assert.That(haveValue, Is.False);

        haveValue = cache.TryGetOneIntProp(original.Id, out var intProp);
        Assert.That(haveValue, Is.True);
        Assert.That(intProp, Is.EqualTo(original.IntProp));
        haveValue = cache.TryGetOneIntProp(99, out _);
        Assert.That(haveValue, Is.False);

        haveValue = cache.TryGetOneInnerClass(original.Id, out var innerClass);
        Assert.That(haveValue, Is.True);
        Assert.That(innerClass, Is.Not.Null);
        Assert.That(Is.ReferenceEquals(innerClass, original.InnerClass), Is.True);

        haveValue = cache.TryGetOneInnerClass(99, out _);
        Assert.That(haveValue, Is.False);
    }

    [Test]
    public void TestDelete()
    {
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystemColumn(NullLogger.Instance, TimeProvider.System, factory);

        var original = new TestClass()
        {
            Id = 12,
            IntProp = 19,
            InnerClass = new InnerTestClass()
            {
                IntProp = 44
            }
        };

        var view = new TestCacheSystemColumn.SetViewOne<TestClass>()
        {
            Id = Enums.SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            IntProp = Enums.SetViewOperation.Set,
            FuncIntProp = static (source) => source.IntProp,
            InnerClass = Enums.SetViewOperation.Set,
            FuncInnerClass = static (source) => source.InnerClass!
        };
        view.Source = original;

        cache.SetOneFromView(original.Id, view);
        cache.DeleteOne(original.Id);

        var haveValue = cache.TryGetOneId(original.Id, out _);
        Assert.That(haveValue, Is.False);

        haveValue = cache.TryGetOneIntProp(original.Id, out _);
        Assert.That(haveValue, Is.False);

        haveValue = cache.TryGetOneInnerClass(original.Id, out _);
        Assert.That(haveValue, Is.False);
    }

    [Test]
    public void TestDeleteColumn()
    {
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystemColumn(NullLogger.Instance, TimeProvider.System, factory);

        var original = new TestClass()
        {
            Id = 12,
            IntProp = 19,
            InnerClass = new InnerTestClass()
            {
                IntProp = 44
            }
        };

        var view = new TestCacheSystemColumn.SetViewOne<TestClass>()
        {
            Id = Enums.SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            IntProp = Enums.SetViewOperation.Set,
            FuncIntProp = static (source) => source.IntProp,
            InnerClass = Enums.SetViewOperation.Set,
            FuncInnerClass = static (source) => source.InnerClass!
        };
        view.Source = original;

        cache.SetOneFromView(original.Id, view);
        cache.DeleteOneIntProp(original.Id);

        var haveValue = cache.TryGetOneId(original.Id, out _);
        Assert.That(haveValue, Is.True);

        haveValue = cache.TryGetOneIntProp(original.Id, out _);
        Assert.That(haveValue, Is.False);

        haveValue = cache.TryGetOneInnerClass(original.Id, out _);
        Assert.That(haveValue, Is.True);
    }

    [Test]
    public void TestRemoveIfExistColumn()
    {
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystemColumn(NullLogger.Instance, TimeProvider.System, factory);

        var original = new TestClass()
        {
            Id = 12,
            IntProp = 19,
            InnerClass = new InnerTestClass()
            {
                IntProp = 44
            }
        };

        var view = new TestCacheSystemColumn.SetViewOne<TestClass>()
        {
            Id = Enums.SetViewOperation.Set,
            FuncId = static (source) => source.Id,
            IntProp = Enums.SetViewOperation.Set,
            FuncIntProp = static (source) => source.IntProp,
            InnerClass = Enums.SetViewOperation.Set,
            FuncInnerClass = static (source) => source.InnerClass!
        };
        view.Source = original;

        cache.SetOneFromView(original.Id, view);

        original.InnerClass = null;
        view.InnerClass = original.InnerClass == null ? Enums.SetViewOperation.RemoveIfExist : Enums.SetViewOperation.Set;

        cache.SetOneFromView(original.Id, view);

        var haveValue = cache.TryGetOneId(original.Id, out _);
        Assert.That(haveValue, Is.True);

        haveValue = cache.TryGetOneIntProp(original.Id, out _);
        Assert.That(haveValue, Is.True);

        haveValue = cache.TryGetOneInnerClass(original.Id, out _);
        Assert.That(haveValue, Is.False);

        view.Id = Enums.SetViewOperation.RemoveIfExist;
        view.IntProp = Enums.SetViewOperation.None;
        view.InnerClass = Enums.SetViewOperation.None;
        cache.SetOneFromView(original.Id, view);

        haveValue = cache.TryGetOneId(original.Id, out _);
        Assert.That(haveValue, Is.False);

        haveValue = cache.TryGetOneIntProp(original.Id, out _);
        Assert.That(haveValue, Is.True);

        haveValue = cache.TryGetOneInnerClass(original.Id, out _);
        Assert.That(haveValue, Is.False);
    }
}