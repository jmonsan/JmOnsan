using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface IAutomationService
{
<<<<<<< HEAD
    Task<TaskExecutionResult> ExecuteAsync(
        string processName,
        CancellationToken cancellationToken = default);
}
=======
    /// <summary>
    /// Runs the process' .bat file and waits for it to finish.
    /// There is no runtime limit: the process is only ever
    /// terminated when <paramref name="cancellationToken"/> is
    /// cancelled (application shutdown).
    /// </summary>
    /// <param name="onOutput">
    /// Optional callback that receives every line the process
    /// writes to standard output / standard error while it runs.
    /// </param>
    Task<TaskExecutionResult> ExecuteAsync(
        string processName,
        Action<string>? onOutput = null,
        CancellationToken cancellationToken = default);
}
>>>>>>> c347f0b (Restore local project)
