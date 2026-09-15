using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface IProcessService
{
    /// <summary>
    /// Returns all available BAT processes.
    /// </summary>
    Task<IReadOnlyCollection<ProcessInfo>> GetProcessesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns process metadata.
    /// </summary>
    Task<ProcessInfo?> GetProcessAsync(
        string processName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads BAT file content.
    /// </summary>
    Task<string> ReadProcessAsync(
        string processName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves BAT file content.
    /// </summary>
    Task SaveProcessAsync(
        string processName,
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a process exists.
    /// </summary>
    Task<bool> ExistsAsync(
        string processName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the absolute path of a process.
    /// Includes path validation.
    /// </summary>
    Task<string> GetProcessPathAsync(
        string processName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates process name and path.
    /// </summary>
    Task ValidateProcessAsync(
        string processName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new BAT file.
    /// </summary>
    Task CreateProcessAsync(
        string processName,
        string content,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a BAT file.
    /// </summary>
    Task DeleteProcessAsync(
        string processName,
        CancellationToken cancellationToken = default);
}