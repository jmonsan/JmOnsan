namespace BotBridge.Core.Models;

/// <summary>
/// Result of a single Execution Logs refresh cycle.
/// </summary>
public sealed class ExecutionLogSnapshot
{
    /// <summary>
    /// Every .txt log file found, newest first.
    /// </summary>
    public IReadOnlyList<ExecutionLogFile> Files { get; init; }
        = Array.Empty<ExecutionLogFile>();

    /// <summary>
    /// False when the UiPath log folder is missing or cannot be read.
    /// </summary>
    public bool FolderAvailable { get; init; }

    /// <summary>
    /// Human-readable reason when the folder is not available.
    /// </summary>
    public string? Error { get; init; }
}
