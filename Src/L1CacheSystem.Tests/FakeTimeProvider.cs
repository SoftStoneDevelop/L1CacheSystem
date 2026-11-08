using System;

namespace L1CacheSystem.Tests;

public class FakeTimeProvider : TimeProvider
{
    public DateTimeOffset FakeTime { get; set; }
    public TimeZoneInfo FakeTimeZone { get; set; } = TimeZoneInfo.Local;

    public FakeTimeProvider(DateTimeOffset time)
    {
        FakeTime = time;
    }

    public override DateTimeOffset GetUtcNow() => FakeTime.ToUniversalTime();

    public override TimeZoneInfo LocalTimeZone => FakeTimeZone;
}