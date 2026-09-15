namespace BotBridge.Core.Models;

public sealed class ExecutionHistoryEntry
{
    public string ProcessName { get; set; }
        = string.Empty;

    public DateTimeOffset ScheduledTime { get; set; }

    public DateTimeOffset ExecutedAt { get; set; }
}
