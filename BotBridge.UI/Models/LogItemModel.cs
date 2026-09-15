namespace BotBridge.UI.Models;

public sealed class LogItemModel
{
    public DateTimeOffset Timestamp { get; set; }

    public string Level { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}