using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

public interface IScheduleService
{
    /// <summary>
    /// Loads previously persisted execution history
    /// (survives app restarts) into the in-memory
    /// duplicate-execution cache. Call once on startup,
    /// before the worker loop begins.
    /// </summary>
    Task LoadHistoryAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the next execution time
    /// for the specified schedule.
    /// </summary>
    DateTimeOffset? GetNextRun(
        ScheduleDefinition schedule,
        DateTimeOffset currentTime);

    /// <summary>
    /// Returns the next scheduled execution
    /// across all configured processes.
    /// </summary>
    ScheduledProcess? GetNextScheduledProcess(
        IEnumerable<ProcessConfig> processes,
        DateTimeOffset currentTime);

    /// <summary>
    /// Determines whether a schedule
    /// is due for execution.
    /// </summary>
    bool IsDue(
        ScheduleDefinition schedule,
        DateTimeOffset currentTime);

    /// <summary>
    /// Determines whether a schedule
    /// was missed while the application
    /// was stopped or sleeping.
    /// </summary>
    bool IsMissedExecution(
        ScheduleDefinition schedule,
        DateTimeOffset lastCheckTime,
        DateTimeOffset currentTime);

    /// <summary>
    /// Prevents duplicate execution
    /// within the same execution window.
    /// </summary>
    bool IsDuplicateExecution(
        string processName,
        DateTimeOffset scheduledTime);

    /// <summary>
    /// Prevents duplicate execution within the same
    /// execution window, honoring the process's
    /// Schedule.PreventDuplicateExecution setting.
    /// </summary>
    bool IsDuplicateExecution(
        ScheduledProcess scheduledProcess);

    /// <summary>
    /// Marks a scheduled execution
    /// as completed.
    /// </summary>
    Task MarkExecutionAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a scheduled execution as completed.
    /// </summary>
    Task MarkExecutionAsync(
        ScheduledProcess scheduledProcess,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes expired execution history.
    /// </summary>
    Task CleanupHistoryAsync(
        CancellationToken cancellationToken = default);
}