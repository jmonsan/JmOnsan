namespace BotBridge.Core.Models;

/// <summary>
/// Immutable snapshot of a single Run Queue entry.
/// </summary>
public sealed class RunQueueItem
{
    public Guid Id { get; init; }

    public string ProcessName { get; init; }
        = string.Empty;

    public ProcessConfig Process { get; init; }
        = new();

    /// <summary>
    /// The time the execution was originally due.
    /// </summary>
    public DateTimeOffset ScheduledTime { get; init; }

    /// <summary>
    /// The time the execution entered the queue.
    /// </summary>
    public DateTimeOffset EnqueuedAt { get; init; }

    public RunQueueStatus Status { get; init; }

    /// <summary>
    /// True when the run was requested manually from the
    /// Processes tab instead of by a schedule.
    /// </summary>
    public bool IsManual { get; init; }

    /// <summary>
    /// True when the entry could not start immediately
    /// (scheduling conflict) and is shown on the Run Queue tab.
    /// </summary>
    public bool IsVisible { get; init; }

    /// <summary>
    /// Number of times execution has been started for this entry.
    /// </summary>
    public int Attempts { get; init; }
}
