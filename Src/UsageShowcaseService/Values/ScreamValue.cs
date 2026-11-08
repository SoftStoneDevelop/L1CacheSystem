using System.Threading;

namespace UsageShowcaseService.Dtos;

public class ScreamValue
{
    private static long _lastId;

    public long Id { get; set; }

    public string? Sound { get; set; }

    public static ScreamValue CopyScream(ScreamValue origin)
    {
        return new ScreamValue()
        {
            Id = origin.Id,
            Sound = origin.Sound
        };
    }

    public static long GetNextId()
    {
        return Interlocked.Increment(ref _lastId);
    }
}