namespace BotBridge.Core.Models;

public sealed class LoggingConfig
{
    public bool Enabled { get; set; } = true;

    public bool LogToFile { get; set; } = true;

    public int RetentionDays { get; set; } = 30;
}