namespace L1CacheSystem.Tests.Model;

public class InnerTestClass
{
    public InnerTestClass()
    {
    }

    public InnerTestClass(InnerTestClass origin)
    {
        IntProp = origin.IntProp;
    }

    public int IntProp { get; set; }
}