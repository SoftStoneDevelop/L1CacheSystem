using System;
using System.Threading;
using System.Threading.Tasks;

namespace L1CacheSystem.Helpers;

public class SemaphoreSlimOwner : IDisposable
{
    private Action? _releaseAction;
    private int _disposed;
    private readonly SemaphoreSlim _semaphore;

    /// <summary>
    /// Constructor for creating a semaphore ownership object
    /// </summary>
    /// <param name="semaphore">semaphore for ownership </param>
    /// <param name="notNeedWait">If false, it attempts to acquire a lock within the constructor.</param>
    /// <param name="releaseAction"> An action that is invoked after Dispose. </param>
    /// <remarks>If, however, an instance is created for the purpose of asynchronous acquisition of ownership, this falls outside the scope of the constructor's responsibility</remarks>
    public SemaphoreSlimOwner(
        SemaphoreSlim semaphore,
        bool notNeedWait,
        Action? releaseAction)
    {
        _releaseAction = releaseAction;
        _semaphore = semaphore ?? throw new ArgumentNullException(nameof(semaphore));
        if (!notNeedWait)
        {
            semaphore.Wait();
        }
    }

    /// <summary>
    /// This method creates a semaphore owner
    /// </summary>
    /// <param name="semaphore">semaphore for ownership</param>
    /// <param name="releaseAction">An action that is invoked after Dispose</param>
    public static async ValueTask<SemaphoreSlimOwner> CreateOwnerAsync(SemaphoreSlim semaphore, Action? releaseAction)
    {
        var enterInLock = semaphore.Wait(0);
        if (enterInLock)
        {
            return new SemaphoreSlimOwner(semaphore, true, releaseAction);
        }
        else
        {
            await semaphore.WaitAsync().ConfigureAwait(false);
            return new SemaphoreSlimOwner(semaphore, true, releaseAction);
        }
    }

    public void Dispose()
    {
        var disposed = Interlocked.CompareExchange(ref _disposed, 1, 0);
        if (disposed == 0)
        {
            try
            {
                _releaseAction?.Invoke();
                _releaseAction = null;

                _semaphore.Release();
            }
            catch
            {
                // ignore
            }

            GC.SuppressFinalize(this);
        }
    }

    ~SemaphoreSlimOwner() => Dispose(); // Protection against an infinite deadlock in case someone forgets to call Dispose somewhere in the code.
}