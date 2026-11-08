using L1CacheSystem;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using UsageShowcaseService.Services.Interfaces;

namespace UsageShowcaseService.HostedServices;

public class EchoSuppressorHostedService : IHostedService
{
    private readonly IServiceCacheSystem _cacheSystem;
    private readonly ILogger<EchoSuppressorHostedService> _logger;

    private CancellationTokenSource? _cts;
    private Task? _supressRoutine;

    public EchoSuppressorHostedService(
        IServiceCacheSystem cacheSystem,
        ILogger<EchoSuppressorHostedService> logger)
    {
        _cacheSystem = cacheSystem;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _supressRoutine = Task.Factory.StartNew(
            async () => await SupressRoutine(_cts.Token),
            _cts.Token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var task = _supressRoutine;
        if (task == null)
        {
            return;
        }

        try
        {
            await _cts!.CancelAsync();
            await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cancellationToken)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }

    private async Task SupressRoutine(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var echos = _cacheSystem.GetAllEcho(returnType: L1CacheSystem.Enums.ReturnType.Copy);
            await Parallel.ForEachAsync(
                echos,
                async (echo, ct) =>
                {
                    await using var lockGuard = new LockGuard(_logger, null);
                    if (string.IsNullOrEmpty(echo.ResidualSound) || echo.ResidualSound.Length <= 1)
                    {
                        await _cacheSystem.GetEchoLockAsync(echo.Id, lockGuard, cancellationToken);
                        var currentEcho = _cacheSystem.GetEcho(echo.Id, returnType: L1CacheSystem.Enums.ReturnType.Reference);
                        if (currentEcho == null || currentEcho.ResidualSound?.Length > 1)
                        {
                            return;
                        }

                        _cacheSystem.DeleteEcho(echo.Id);
                    }
                    else
                    {
                        await _cacheSystem.GetEchoLockAsync(echo.Id, lockGuard, cancellationToken);
                        var currentEcho = _cacheSystem.GetEcho(echo.Id, returnType: L1CacheSystem.Enums.ReturnType.Copy);
                        if (echo.ResidualSound?.Length > 1)
                        {
                            echo.ResidualSound = echo.ResidualSound.Substring(0, echo.ResidualSound.Length - 1);
                        }

                        _cacheSystem.SetEcho(echo, L1CacheSystem.Enums.ValueOwnership.Transferred);
                    }
                });

            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }
}