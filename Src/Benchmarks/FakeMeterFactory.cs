using System.Diagnostics.Metrics;

namespace Benchmarks;

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