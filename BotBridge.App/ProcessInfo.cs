namespace BotBridge.Core.Models;

public sealed class ProcessInfo
{
    public string Name { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public long SizeInBytes { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset LastModifiedAt { get; set; }

    public bool Exists { get; set; }
}
