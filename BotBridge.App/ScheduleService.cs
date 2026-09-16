using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;
using BotBridge.Infrastructure.Persistence;

namespace BotBridge.Application.Services;

public sealed class ScheduleService : IScheduleService
{
    private readonly IExecutionHistoryStore
        _executionHistoryStore;

    private readonly Dictionary<string, DateTimeOffset>
        _executionHistory = new();

    private bool _historyLoaded;

    public ScheduleService(
        IExecutionHistoryStore executionHistoryStore)
    {
        _executionHistoryStore =
            executionHistoryStore;
    }

    /// <summary>
    /// Loads persisted execution-history.json into the
    /// in-memory duplicate-execution cache. Safe to call
    /// more than once; only loads from disk the first time.
    /// </summary>
    public async Task LoadHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        if (_historyLoaded)
        {
            return;
        }

        var records =
            await _executionHistoryStore
                .GetAllAsync(cancellationToken);

        foreach (var group in records
                     .GroupBy(x => x.ProcessName))
        {
            var latest =
                group
                    .OrderByDescending(x => x.ScheduledTime)
                    .First();

            _executionHistory[group.Key] =
                latest.ScheduledTime;
        }

        _historyLoaded = true;
    }

    public DateTimeOffset? GetNextRun(
        ScheduleDefinition schedule,
        DateTimeOffset currentTime)
    {
        if (!schedule.Enabled)
        {
            return null;
        }

        return schedule.Type switch
        {
            ScheduleType.Once =>
                GetNextOnceRun(
                    schedule,
                    currentTime),

            ScheduleType.Daily =>
                GetNextDailyRun(
                    schedule,
                    currentTime),

            ScheduleType.Weekly =>
                GetNextWeeklyRun(
                    schedule,
                    currentTime),

            _ => null
        };
    }

    public ScheduledProcess? GetNextScheduledProcess(
        IEnumerable<ProcessConfig> processes,
        DateTimeOffset currentTime)
    {
        return processes
            .Where(x =>
                x.Enabled &&
                x.Schedule.Enabled)
            .Select(x => new
            {
                Process = x,
                NextRun = GetNextRun(
                    x.Schedule,
                    currentTime)
            })
            .Where(x => x.NextRun.HasValue)
            .OrderBy(x => x.NextRun)
            .Select(x => new ScheduledProcess
            {
                ProcessName = x.Process.Name,
                Process = x.Process,
                ScheduledTime = x.NextRun!.Value
            })
            .FirstOrDefault();
    }

    public bool IsDue(
        ScheduleDefinition schedule,
        DateTimeOffset currentTime)
    {
        var nextRun =
            GetNextRun(
                schedule,
                currentTime.AddSeconds(-1));

        if (!nextRun.HasValue)
        {
            return false;
        }

        return currentTime >= nextRun.Value;
    }

    public bool IsMissedExecution(
        ScheduleDefinition schedule,
        DateTimeOffset lastCheckTime,
        DateTimeOffset currentTime)
    {
        var nextRun =
            GetNextRun(
                schedule,
                lastCheckTime);

        if (!nextRun.HasValue)
        {
            return false;
        }

        return nextRun.Value > lastCheckTime &&
               nextRun.Value <= currentTime;
    }

    public bool IsDuplicateExecution(
        string processName,
        DateTimeOffset scheduledTime)
    {
        if (!_executionHistory.TryGetValue(
                processName,
                out var lastRun))
        {
            return false;
        }

        return lastRun == scheduledTime;
    }

    public bool IsDuplicateExecution(
        ScheduledProcess scheduledProcess)
    {
        ArgumentNullException.ThrowIfNull(
            scheduledProcess);

        if (!scheduledProcess.Process.Schedule
                .PreventDuplicateExecution)
        {
            return false;
        }

        return IsDuplicateExecution(
            scheduledProcess.ProcessName,
            scheduledProcess.ScheduledTime);
    }

    public async Task MarkExecutionAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
    {
        _executionHistory[processName] =
            scheduledTime;

        // Write-through to execution-history.json so this
        // survives an app restart.
        await _executionHistoryStore.MarkExecutedAsync(
            processName,
            scheduledTime,
            cancellationToken);
    }

    public Task MarkExecutionAsync(
        ScheduledProcess scheduledProcess,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            scheduledProcess);

        return MarkExecutionAsync(
            scheduledProcess.ProcessName,
            scheduledProcess.ScheduledTime,
            cancellationToken);
    }

    public async Task CleanupHistoryAsync(
        CancellationToken cancellationToken = default)
    {
        var cutoff =
            DateTimeOffset.Now.AddDays(-30);

        var expired =
            _executionHistory
                .Where(x => x.Value < cutoff)
                .Select(x => x.Key)
                .ToList();

        foreach (var key in expired)
        {
            _executionHistory.Remove(key);
        }

        // Keep execution-history.json in sync with the
        // same 30-day retention window.
        await _executionHistoryStore.CleanupAsync(
            TimeSpan.FromDays(30),
            cancellationToken);
    }

    private static DateTimeOffset? GetNextOnceRun(
        ScheduleDefinition schedule,
        DateTimeOffset currentTime)
    {
        if (schedule.StartDateTime <= currentTime)
        {
            return null;
        }

        return schedule.StartDateTime;
    }

    private static DateTimeOffset GetNextDailyRun(
        ScheduleDefinition schedule,
        DateTimeOffset currentTime)
    {
        var scheduledTime =
            schedule.StartDateTime.TimeOfDay;

        var todayRun =
            new DateTimeOffset(
                currentTime.Year,
                currentTime.Month,
                currentTime.Day,
                scheduledTime.Hours,
                scheduledTime.Minutes,
                scheduledTime.Seconds,
                currentTime.Offset);

        if (todayRun > currentTime)
        {
            return todayRun;
        }

        return todayRun.AddDays(1);
    }

private static DateTimeOffset GetNextWeeklyRun(
    ScheduleDefinition schedule,
    DateTimeOffset currentTime)
{
    var targetDay =
        schedule.DayOfWeek ??
        schedule.StartDateTime.DayOfWeek;

    var scheduledTime =
        schedule.StartDateTime.TimeOfDay;

    var daysUntil =
        ((int)targetDay -
         (int)currentTime.DayOfWeek + 7) % 7;

    var candidate =
        new DateTimeOffset(
            currentTime.Year,
            currentTime.Month,
            currentTime.Day,
            scheduledTime.Hours,
            scheduledTime.Minutes,
            scheduledTime.Seconds,
            currentTime.Offset)
        .AddDays(daysUntil);

    if (candidate <= currentTime)
    {
        candidate = candidate.AddDays(7);
    }

    return candidate;
}}