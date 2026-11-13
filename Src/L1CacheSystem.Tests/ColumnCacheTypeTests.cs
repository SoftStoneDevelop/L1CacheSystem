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
[KeyColumnCache(
    keyType: typeof(long),
    interfaceType: typeof(IL1CacheSystemColumn),
    cacheName: "Columns33"),
    ColumnCache(columnType: typeof(long), columnName: "p1"),
    ColumnCache(columnType: typeof(long), columnName: "p2"),
    ColumnCache(columnType: typeof(long), columnName: "p3"),
    ColumnCache(columnType: typeof(long), columnName: "p4"),
    ColumnCache(columnType: typeof(long), columnName: "p5"),
    ColumnCache(columnType: typeof(long), columnName: "p6"),
    ColumnCache(columnType: typeof(long), columnName: "p7"),
    ColumnCache(columnType: typeof(long), columnName: "p8"),
    ColumnCache(columnType: typeof(long), columnName: "p9"),
    ColumnCache(columnType: typeof(long), columnName: "p10"),
    ColumnCache(columnType: typeof(long), columnName: "p11"),
    ColumnCache(columnType: typeof(long), columnName: "p12"),
    ColumnCache(columnType: typeof(long), columnName: "p13"),
    ColumnCache(columnType: typeof(long), columnName: "p14"),
    ColumnCache(columnType: typeof(long), columnName: "p15"),
    ColumnCache(columnType: typeof(long), columnName: "p16"),
    ColumnCache(columnType: typeof(long), columnName: "p17"),
    ColumnCache(columnType: typeof(long), columnName: "p18"),
    ColumnCache(columnType: typeof(long), columnName: "p19"),
    ColumnCache(columnType: typeof(long), columnName: "p20"),
    ColumnCache(columnType: typeof(long), columnName: "p21"),
    ColumnCache(columnType: typeof(long), columnName: "p22"),
    ColumnCache(columnType: typeof(long), columnName: "p23"),
    ColumnCache(columnType: typeof(long), columnName: "p24"),
    ColumnCache(columnType: typeof(long), columnName: "p25"),
    ColumnCache(columnType: typeof(long), columnName: "p26"),
    ColumnCache(columnType: typeof(long), columnName: "p27"),
    ColumnCache(columnType: typeof(long), columnName: "p28"),
    ColumnCache(columnType: typeof(long), columnName: "p29"),
    ColumnCache(columnType: typeof(long), columnName: "p30"),
    ColumnCache(columnType: typeof(long), columnName: "p31"),
    ColumnCache(columnType: typeof(long), columnName: "p32"),
    ColumnCache(columnType: typeof(long), columnName: "p33")]
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

        cache.ViewInContextOne((in context) =>
        {
            var haveValue = context.TryGetOneId(original.Id, out var id);
            Assert.That(haveValue, Is.True);
            Assert.That(id, Is.EqualTo(original.Id));
            haveValue = context.TryGetOneId(99, out _);
            Assert.That(haveValue, Is.False);

            haveValue = context.TryGetOneIntProp(original.Id, out var intProp);
            Assert.That(haveValue, Is.True);
            Assert.That(intProp, Is.EqualTo(original.IntProp));
            haveValue = context.TryGetOneIntProp(99, out _);
            Assert.That(haveValue, Is.False);

            haveValue = context.TryGetOneInnerClass(original.Id, out var innerClass);
            Assert.That(haveValue, Is.True);
            Assert.That(innerClass, Is.Not.Null);
            Assert.That(Is.ReferenceEquals(innerClass, original.InnerClass), Is.True);

            haveValue = context.TryGetOneInnerClass(99, out _);
            Assert.That(haveValue, Is.False);
        });
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

        cache.ViewInContextOne((in context) =>
        {
            var haveValue = context.TryGetOneId(original.Id, out _);
            Assert.That(haveValue, Is.False);

            haveValue = context.TryGetOneIntProp(original.Id, out _);
            Assert.That(haveValue, Is.False);

            haveValue = context.TryGetOneInnerClass(original.Id, out _);
            Assert.That(haveValue, Is.False);
        });
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

        cache.ViewInContextOne((in context) =>
        {
            var haveValue = context.TryGetOneId(original.Id, out _);
            Assert.That(haveValue, Is.True);

            haveValue = context.TryGetOneIntProp(original.Id, out _);
            Assert.That(haveValue, Is.False);

            haveValue = context.TryGetOneInnerClass(original.Id, out _);
            Assert.That(haveValue, Is.True);
        });
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

        cache.ViewInContextOne((in context) =>
        {
            var haveValue = context.TryGetOneId(original.Id, out _);
            Assert.That(haveValue, Is.True);

            haveValue = context.TryGetOneIntProp(original.Id, out _);
            Assert.That(haveValue, Is.True);

            haveValue = context.TryGetOneInnerClass(original.Id, out _);
            Assert.That(haveValue, Is.False);
        });

        view.Id = Enums.SetViewOperation.RemoveIfExist;
        view.IntProp = Enums.SetViewOperation.None;
        view.InnerClass = Enums.SetViewOperation.None;
        cache.SetOneFromView(original.Id, view);

        cache.ViewInContextOne((in context) =>
        {
            var haveValue = context.TryGetOneId(original.Id, out _);
            Assert.That(haveValue, Is.False);

            haveValue = context.TryGetOneIntProp(original.Id, out _);
            Assert.That(haveValue, Is.True);

            haveValue = context.TryGetOneInnerClass(original.Id, out _);
            Assert.That(haveValue, Is.False);
        });
    }

    [Test]
    public void TestClearExpiredCache()
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);
        var factory = new FakeMeterFactory();
        var cache = new TestCacheSystemColumn(NullLogger.Instance, timeProvider, factory);

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

        cache.SetOneFromView(original.Id, view, TimeSpan.FromSeconds(10));
        cache.ClearExpiredCache();

        cache.ViewInContextOne((in context) =>
        {
            var have = context.TryGetOneId(original.Id, out _);
            Assert.That(have, Is.True);

            timeProvider.FakeTime = timeProvider.FakeTime.AddSeconds(1); // 9 remind
            cache.ClearExpiredCache();
            have = context.TryGetOneId(original.Id, out _);
            Assert.That(have, Is.True);

            timeProvider.FakeTime = timeProvider.FakeTime.AddSeconds(5); // 4 remind
            cache.ClearExpiredCache();
            have = context.TryGetOneId(original.Id, out _);
            Assert.That(have, Is.True);

            timeProvider.FakeTime = timeProvider.FakeTime.AddSeconds(3); // 1 remind
            cache.ClearExpiredCache();
            have = context.TryGetOneId(original.Id, out _);
            Assert.That(have, Is.True);

            timeProvider.FakeTime = timeProvider.FakeTime.AddSeconds(1); // 0 remind but we live
            cache.ClearExpiredCache();
            have = context.TryGetOneId(original.Id, out _);
            Assert.That(have, Is.True);

            timeProvider.FakeTime = timeProvider.FakeTime.AddMicroseconds(1); // expired
            cache.ClearExpiredCache();
            have = context.TryGetOneId(original.Id, out _);
            Assert.That(have, Is.False);
        });
    }

    #region SparseIndex tests

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void GetIndex(bool inlinedArray)
    {
        if (inlinedArray)
        {
            var si = new TestCacheSystemColumn.SparseIndexOne();
            Assert.That(si.IsInlinedArray(), Is.True);

            si.SetIndex(0, true, 64);
            Assert.That(si.GetIndex(0), Is.EqualTo(64));
            Assert.That(si.GetIndex(1), Is.EqualTo(-1));
            Assert.That(si.GetIndex(2), Is.EqualTo(-1));

            si.SetIndex(1, true, 49);
            Assert.That(si.GetIndex(0), Is.EqualTo(64));
            Assert.That(si.GetIndex(1), Is.EqualTo(49));
            Assert.That(si.GetIndex(2), Is.EqualTo(-1));

            si.SetIndex(2, true, 87);
            Assert.That(si.GetIndex(0), Is.EqualTo(64));
            Assert.That(si.GetIndex(1), Is.EqualTo(49));
            Assert.That(si.GetIndex(2), Is.EqualTo(87));

            si.SetIndex(1, false, 164);
            Assert.That(si.GetIndex(0), Is.EqualTo(64));
            Assert.That(si.GetIndex(1), Is.EqualTo(-1));
            Assert.That(si.GetIndex(2), Is.EqualTo(87));

            si.SetIndex(1, true, 164);
            Assert.That(si.GetIndex(0), Is.EqualTo(64));
            Assert.That(si.GetIndex(1), Is.EqualTo(164));
            Assert.That(si.GetIndex(2), Is.EqualTo(87));

            si.SetIndex(1, false, 118);
            si.SetIndex(0, false, 115);
            Assert.That(si.GetIndex(0), Is.EqualTo(-1));
            Assert.That(si.GetIndex(1), Is.EqualTo(-1));
            Assert.That(si.GetIndex(2), Is.EqualTo(87));

            si.SetIndex(1, false, 118);
            si.SetIndex(0, false, 115);
            si.SetIndex(2, false, 406);
            Assert.That(si.GetIndex(0), Is.EqualTo(-1));
            Assert.That(si.GetIndex(1), Is.EqualTo(-1));
            Assert.That(si.GetIndex(2), Is.EqualTo(-1));
        }
        else
        {
            var si = new TestCacheSystemColumn.SparseIndexColumns33();
            Assert.That(si.IsInlinedArray(), Is.False);

            si.SetIndex(1, true, 64);
            Assert.That(si.GetIndex(1), Is.EqualTo(64));
            Assert.That(si.GetIndex(3), Is.EqualTo(-1));
            Assert.That(si.GetIndex(10), Is.EqualTo(-1));

            si.SetIndex(3, true, 49);
            Assert.That(si.GetIndex(1), Is.EqualTo(64));
            Assert.That(si.GetIndex(3), Is.EqualTo(49));
            Assert.That(si.GetIndex(10), Is.EqualTo(-1));

            si.SetIndex(10, true, 87);
            Assert.That(si.GetIndex(1), Is.EqualTo(64));
            Assert.That(si.GetIndex(3), Is.EqualTo(49));
            Assert.That(si.GetIndex(10), Is.EqualTo(87));

            si.SetIndex(3, false, 164);
            Assert.That(si.GetIndex(1), Is.EqualTo(64));
            Assert.That(si.GetIndex(3), Is.EqualTo(-1));
            Assert.That(si.GetIndex(10), Is.EqualTo(87));

            si.SetIndex(3, true, 164);
            Assert.That(si.GetIndex(1), Is.EqualTo(64));
            Assert.That(si.GetIndex(3), Is.EqualTo(164));
            Assert.That(si.GetIndex(10), Is.EqualTo(87));

            si.SetIndex(3, false, 118);
            si.SetIndex(1, false, 115);
            Assert.That(si.GetIndex(1), Is.EqualTo(-1));
            Assert.That(si.GetIndex(3), Is.EqualTo(-1));
            Assert.That(si.GetIndex(10), Is.EqualTo(87));

            si.SetIndex(3, false, 118);
            si.SetIndex(1, false, 115);
            si.SetIndex(10, false, 406);
            Assert.That(si.GetIndex(1), Is.EqualTo(-1));
            Assert.That(si.GetIndex(3), Is.EqualTo(-1));
            Assert.That(si.GetIndex(10), Is.EqualTo(-1));
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void HavePropCount(bool inlinedArray)
    {
        if (inlinedArray)
        {
            var si = new TestCacheSystemColumn.SparseIndexOne();
            Assert.That(si.IsInlinedArray(), Is.True);

            si.SetIndex(0, true, 64);
            Assert.That(si.HavePropCount(), Is.EqualTo(1));

            si.SetIndex(1, true, 49);
            Assert.That(si.HavePropCount(), Is.EqualTo(2));

            si.SetIndex(2, true, 87);
            Assert.That(si.HavePropCount(), Is.EqualTo(3));

            si.SetIndex(1, false, 164);
            Assert.That(si.HavePropCount(), Is.EqualTo(2));

            si.SetIndex(1, true, 164);
            Assert.That(si.HavePropCount(), Is.EqualTo(3));

            si.SetIndex(1, false, 118);
            si.SetIndex(0, false, 115);
            Assert.That(si.HavePropCount(), Is.EqualTo(1));

            si.SetIndex(1, false, 118);
            si.SetIndex(0, false, 115);
            si.SetIndex(2, false, 406);
            Assert.That(si.HavePropCount(), Is.Zero);
        }
        else
        {
            var si = new TestCacheSystemColumn.SparseIndexColumns33();
            Assert.That(si.IsInlinedArray(), Is.False);

            si.SetIndex(1, true, 64);
            Assert.That(si.HavePropCount(), Is.EqualTo(1));

            si.SetIndex(3, true, 49);
            Assert.That(si.HavePropCount(), Is.EqualTo(2));

            si.SetIndex(10, true, 87);
            Assert.That(si.HavePropCount(), Is.EqualTo(3));

            si.SetIndex(3, false, 164);
            Assert.That(si.HavePropCount(), Is.EqualTo(2));

            si.SetIndex(3, true, 164);
            Assert.That(si.HavePropCount(), Is.EqualTo(3));

            si.SetIndex(3, false, 118);
            si.SetIndex(1, false, 115);
            Assert.That(si.HavePropCount(), Is.EqualTo(1));

            si.SetIndex(3, false, 118);
            si.SetIndex(1, false, 115);
            si.SetIndex(10, false, 406);
            Assert.That(si.HavePropCount(), Is.Zero);
        }
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void HaveProp(bool inlinedArray)
    {
        if (inlinedArray)
        {
            var si = new TestCacheSystemColumn.SparseIndexOne();
            Assert.That(si.IsInlinedArray(), Is.True);

            si.SetIndex(0, true, 64);
            Assert.That(si.HaveProp(0), Is.True);
            Assert.That(si.HaveProp(1), Is.False);
            Assert.That(si.HaveProp(2), Is.False);

            si.SetIndex(1, true, 49);
            Assert.That(si.HaveProp(0), Is.True);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(2), Is.False);

            si.SetIndex(2, true, 87);
            Assert.That(si.HaveProp(0), Is.True);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(2), Is.True);

            si.SetIndex(1, false, 164);
            Assert.That(si.HaveProp(0), Is.True);
            Assert.That(si.HaveProp(1), Is.False);
            Assert.That(si.HaveProp(2), Is.True);

            si.SetIndex(1, true, 164);
            Assert.That(si.HaveProp(0), Is.True);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(2), Is.True);

            si.SetIndex(1, false, 118);
            si.SetIndex(0, false, 115);
            Assert.That(si.HaveProp(0), Is.False);
            Assert.That(si.HaveProp(1), Is.False);
            Assert.That(si.HaveProp(2), Is.True);

            si.SetIndex(1, false, 118);
            si.SetIndex(0, false, 115);
            si.SetIndex(2, false, 406);
            Assert.That(si.HaveProp(0), Is.False);
            Assert.That(si.HaveProp(1), Is.False);
            Assert.That(si.HaveProp(2), Is.False);
        }
        else
        {
            var si = new TestCacheSystemColumn.SparseIndexColumns33();
            Assert.That(si.IsInlinedArray(), Is.False);

            si.SetIndex(1, true, 64);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(3), Is.False);
            Assert.That(si.HaveProp(10), Is.False);

            si.SetIndex(3, true, 49);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(3), Is.True);
            Assert.That(si.HaveProp(10), Is.False);

            si.SetIndex(10, true, 87);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(3), Is.True);
            Assert.That(si.HaveProp(10), Is.True);

            si.SetIndex(3, false, 164);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(3), Is.False);
            Assert.That(si.HaveProp(10), Is.True);

            si.SetIndex(3, true, 164);
            Assert.That(si.HaveProp(1), Is.True);
            Assert.That(si.HaveProp(3), Is.True);
            Assert.That(si.HaveProp(10), Is.True);

            si.SetIndex(3, false, 118);
            si.SetIndex(1, false, 115);
            Assert.That(si.HaveProp(1), Is.False);
            Assert.That(si.HaveProp(3), Is.False);
            Assert.That(si.HaveProp(10), Is.True);

            si.SetIndex(3, false, 118);
            si.SetIndex(1, false, 115);
            si.SetIndex(10, false, 406);
            Assert.That(si.HaveProp(1), Is.False);
            Assert.That(si.HaveProp(3), Is.False);
            Assert.That(si.HaveProp(10), Is.False);
        }
    }

    #endregion
}