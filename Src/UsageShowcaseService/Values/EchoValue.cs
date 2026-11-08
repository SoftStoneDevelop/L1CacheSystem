using System.Threading;

namespace UsageShowcaseService.Values;

public class EchoValue
{
    private static long _lastId;

    public long Id { get; set; }

    public string? ResidualSound { get; set; }

    public EchoValue Copy()
    {
        return new EchoValue()
        {
            Id = this.Id,
            ResidualSound = this.ResidualSound
        };
    }

    public static long GetNextId()
    {
        return Interlocked.Increment(ref _lastId);
    }
}