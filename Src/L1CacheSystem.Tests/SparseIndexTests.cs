namespace L1CacheSystem.Tests;

internal class SparseIndexTests
{
    [Test]
    public void GetIndex()
    {
        var si = new L1CacheSystemBase.SparseIndex();

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

    [Test]
    public void HavePropCount()
    {
        var si = new L1CacheSystemBase.SparseIndex();

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

    [Test]
    public void HaveProp()
    {
        var si = new L1CacheSystemBase.SparseIndex();

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