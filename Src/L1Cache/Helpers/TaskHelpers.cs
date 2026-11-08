using System;
using System.Threading;
using System.Threading.Tasks;

namespace L1CacheSystem.Helpers;

internal static class TaskHelpers
{
    /// <summary>
    /// Wait for a task to complete or cancel the token
    /// </summary>
    internal static async Task WaitEndRoutine(Task? routine, CancellationToken cancellation = default)
    {
        if (routine == null)
        {
            return;
        }

        try
        {
            await Task.WhenAny(routine, Task.Delay(Timeout.Infinite, cancellation)).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }
}