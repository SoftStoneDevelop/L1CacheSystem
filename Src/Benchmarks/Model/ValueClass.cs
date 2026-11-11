namespace Benchmarks.Model;

public class ValueClass
{
    public long Id { get; set; }

    public int IntProp { get; set; }

    public double? DoubleProp { get; set; }

    public ValueClass Child { get; set; }

    public static ValueClass MakeCopy(ValueClass origin)
    {
        return new ValueClass
        {
            Id = origin.Id,
            IntProp = origin.IntProp,
            Child = origin.Child == null ? null : MakeCopy(origin.Child),
            DoubleProp = origin.DoubleProp
        };
    }
}