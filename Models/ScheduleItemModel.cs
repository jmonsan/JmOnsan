namespace BotBridge.UI.Models;

public sealed class ScheduleItemModel
{
    public string ProcessName { get; set; }
        = string.Empty;

    public string ScheduleType { get; set; }
        = string.Empty;

    public DateTimeOffset StartDateTime { get; set; }

    public bool Enabled { get; set; }
}