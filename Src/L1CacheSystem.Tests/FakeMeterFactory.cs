using System.Diagnostics.Metrics;

namespace L1CacheSystem.Tests;

public class FakeMeterFactory : IMeterFactory
{
    public Meter Create(MeterOptions options)
    {
        return new Meter(options);
    }

    public void Dispose()
    {
    }
}