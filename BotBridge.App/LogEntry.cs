namespace BotBridge.Core.Models;

public sealed class LogEntry
{
    public DateTimeOffset Timestamp { get; set; }

    public string Level { get; set; }
        = string.Empty;

    public string Message { get; set; }
        = string.Empty;
}