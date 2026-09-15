namespace BotBridge.Core.Models;

public sealed class ScheduleDefinition
{
    /// <summary>
    /// Schedule frequency.
    /// </summary>
    public ScheduleType Type { get; set; } = ScheduleType.Once;

    /// <summary>
    /// First execution date/time.
    /// For Daily schedules, only the time portion is reused.
    /// For Weekly schedules, both time and day-of-week are used.
    /// </summary>
    public DateTimeOffset StartDateTime { get; set; }

    /// <summary>
    /// Enable/disable this schedule.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Run missed executions after application restart.
    /// </summary>
    public bool RunMissedExecution { get; set; } = true;

    /// <summary>
    /// Prevent duplicate execution within the same schedule window.
    /// </summary>
    public bool PreventDuplicateExecution { get; set; } = true;

    /// <summary>
    /// Maximum allowed delay before a schedule is considered expired.
    /// </summary>
    public TimeSpan MaxLateExecutionWindow { get; set; }
        = TimeSpan.FromHours(24);

    /// <summary>
    /// Used only for weekly schedules.
    /// Defaults to StartDateTime.DayOfWeek.
    /// </summary>
    public DayOfWeek? DayOfWeek { get; set; }

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}