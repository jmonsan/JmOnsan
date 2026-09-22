namespace BotBridge.Core.Models;

/// <summary>
/// A UiPath execution log file (.txt) discovered on disk.
/// </summary>
public sealed class ExecutionLogFile
{
    public string FileName { get; set; }
        = string.Empty;

    public string FullPath { get; set; }
        = string.Empty;

    public DateTimeOffset LastModified { get; set; }

    public long SizeInBytes { get; set; }
}
