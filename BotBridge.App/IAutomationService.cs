using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface IAutomationService
{
    Task<TaskExecutionResult> ExecuteAsync(
        string processName,
        CancellationToken cancellationToken = default);
}