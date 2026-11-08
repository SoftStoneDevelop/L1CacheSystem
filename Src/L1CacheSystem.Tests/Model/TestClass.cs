namespace L1CacheSystem.Tests.Model;

public class TestClass
{
    public TestClass()
    {
    }

    public TestClass(TestClass origin)
    {
        Id = origin.Id;
        IntProp = origin.IntProp;
        InnerClass = origin.InnerClass == null ? null : new InnerTestClass(origin.InnerClass);
    }

    public long Id { get; set; }

    public int IntProp { get; set; }

    public InnerTestClass? InnerClass { get; set; }

    public TestClass MySuperCopyMethod()
    {
        return new TestClass
        {
            Id = this.Id,
            IntProp = this.IntProp,
            InnerClass = this.InnerClass == null ? null : new InnerTestClass(this.InnerClass)
        };
    }

    public static TestClass MySuperStaticCopyMethod(TestClass origin)
    {
        return new TestClass
        {
            Id = origin.Id,
            IntProp = origin.IntProp,
            InnerClass = origin.InnerClass == null ? null : new InnerTestClass(origin.InnerClass)
        };
    }
}