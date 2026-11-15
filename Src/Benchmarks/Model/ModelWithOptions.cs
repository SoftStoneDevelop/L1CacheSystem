namespace Benchmarks.Model;

public class ModelWithOptions
{
    public long Id { get; set; }

    public double? DoubleProp1 { get; set; }

    public double? DoubleProp2 { get; set; }

    public double? DoubleProp3 { get; set; }

    public double? DoubleProp4 { get; set; }

    public double? DoubleProp5 { get; set; }

    public double? DoubleProp6 { get; set; }

    public double? DoubleProp7 { get; set; }

    public double? DoubleProp8 { get; set; }

    public double? DoubleProp9 { get; set; }

    public double? DoubleProp10 { get; set; }

    public static ModelWithOptions MakeCopy(ModelWithOptions origin)
    {
        return new ModelWithOptions
        {
            Id = origin.Id,
            DoubleProp1 = origin.DoubleProp1,
            DoubleProp2 = origin.DoubleProp2,
            DoubleProp3 = origin.DoubleProp3,
            DoubleProp4 = origin.DoubleProp4,
            DoubleProp5 = origin.DoubleProp5,
            DoubleProp6 = origin.DoubleProp6,
            DoubleProp7 = origin.DoubleProp7,
            DoubleProp8 = origin.DoubleProp8,
            DoubleProp9 = origin.DoubleProp9,
            DoubleProp10 = origin.DoubleProp10,
        };
    }
}