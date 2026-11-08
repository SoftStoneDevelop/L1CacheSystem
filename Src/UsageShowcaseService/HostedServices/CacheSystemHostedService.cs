using L1CacheSystem;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace UsageShowcaseService.HostedServices;

public class CacheSystemHostedService : IHostedService
{
    private readonly IL1CacheSystemBase _cacheSystem;

    public CacheSystemHostedService(IL1CacheSystemBase cacheSystem)
    {
        _cacheSystem = cacheSystem;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _cacheSystem.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _cacheSystem.StopAsync(cancellationToken);
    }
}