namespace BotBridge.Core.Models;

public sealed class ScheduledProcess
{
    public string ProcessName { get; set; }
        = string.Empty;

    public ProcessConfig Process { get; set; }
        = null!;

    public DateTimeOffset ScheduledTime { get; set; }

    public TimeSpan TimeUntilRun =>
        ScheduledTime - DateTimeOffset.Now;
}