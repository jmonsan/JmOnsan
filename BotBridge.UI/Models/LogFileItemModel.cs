namespace BotBridge.UI.Models;

public sealed class LogFileItemModel
{
    public string FileName { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public DateTimeOffset LastModified { get; set; }

    public long SizeInBytes { get; set; }
}
