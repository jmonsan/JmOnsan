using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface ILoggerService
{
    Task InfoAsync(
        string message,
        CancellationToken cancellationToken = default);

    Task WarningAsync(
        string message,
        CancellationToken cancellationToken = default);

    Task ErrorAsync(
        string message,
        CancellationToken cancellationToken = default);

    Task ErrorAsync(
        Exception exception,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LogEntry>>
        GetRecentLogsAsync(
            int count = 100,
            CancellationToken cancellationToken = default);
}


