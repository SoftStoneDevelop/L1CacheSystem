using System;
using System.Threading;
using System.Threading.Tasks;

namespace L1CacheSystem;

public interface IL1CacheSystemBase : IDisposable
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}