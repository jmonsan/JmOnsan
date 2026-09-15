using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface IProcessService
{
Task<IReadOnlyCollection<ProcessInfo>>
    GetProcessesAsync(
        CancellationToken cancellationToken = default);

Task<ProcessInfo?>
    GetProcessAsync(
        string processName,
        CancellationToken cancellationToken = default);

Task<string>
    ReadProcessAsync(
        string processName,
        CancellationToken cancellationToken = default);

Task SaveProcessAsync(
    string processName,
    string content,
    CancellationToken cancellationToken = default);

Task<bool>
    ExistsAsync(
        string processName,
        CancellationToken cancellationToken = default);

Task<string>
    GetProcessPathAsync(
        string processName,
        CancellationToken cancellationToken = default);

Task ValidateProcessAsync(
    string processName,
    CancellationToken cancellationToken = default);

Task CreateProcessAsync(
    string processName,
    string content,
    CancellationToken cancellationToken = default);

Task DeleteProcessAsync(
    string processName,
    CancellationToken cancellationToken = default);

}