using BotBridge.Core.Models;

namespace BotBridge.Infrastructure.Persistence;

public interface IExecutionHistoryStore
{
    Task<bool> IsExecutedAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default);

    Task MarkExecutedAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ExecutionHistoryEntry>>
        GetAllAsync(
            CancellationToken cancellationToken = default);

    Task CleanupAsync(
        TimeSpan retention,
        CancellationToken cancellationToken = default);
}