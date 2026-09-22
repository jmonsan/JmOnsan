using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

/// <summary>
/// Reads the UiPath execution logs (.txt files) and keeps an
/// incremental, de-duplicated view of their content.
/// </summary>
public interface IExecutionLogService
{
    /// <summary>
    /// Folder that is scanned for .txt log files.
    /// </summary>
    string FolderPath { get; }

    /// <summary>
    /// Scans the folder and reads any new content. Safe to call
    /// repeatedly and from multiple threads; concurrent calls are
    /// serialized.
    /// </summary>
    Task<ExecutionLogSnapshot> RefreshAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the current content of a log file.
    /// </summary>
    Task<string> GetContentAsync(
        string fullPath,
        CancellationToken cancellationToken = default);
}
