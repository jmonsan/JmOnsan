<<<<<<< HEAD
=======
using System.Collections.Concurrent;
>>>>>>> c347f0b (Restore local project)
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Workers;

<<<<<<< HEAD
=======
/// <summary>
/// Drives BotBridge automation:
///
///  - the scheduler loop turns due schedules into queued executions,
///  - the runner starts queued executions whenever a slot is free,
///  - a due execution that collides with a running process is never
///    skipped and the running process is never killed: it waits in the
///    Run Queue until it can start.
///
/// Processes have no runtime limit. The only thing that ever terminates
/// a running process is application shutdown.
/// </summary>
>>>>>>> c347f0b (Restore local project)
public sealed class AutomationWorker
{
    private static readonly TimeSpan ConfigRecheckInterval =
        TimeSpan.FromSeconds(30);

<<<<<<< HEAD
=======
    private static readonly TimeSpan SchedulerTickInterval =
        TimeSpan.FromSeconds(1);

    private static readonly TimeSpan RunnerTickInterval =
        TimeSpan.FromSeconds(1);

    /// <summary>
    /// How long to wait before retrying an execution that was
    /// blocked by another UiPath foreground process.
    /// </summary>
    private static readonly TimeSpan ConflictRetryDelay =
        TimeSpan.FromSeconds(30);

    /// <summary>
    /// A schedule that is created or edited while the worker is
    /// running may be due up to this long ago and still run.
    /// </summary>
    private static readonly TimeSpan NewScheduleGracePeriod =
        TimeSpan.FromMinutes(1);

    private static readonly TimeSpan HistoryCleanupInterval =
        TimeSpan.FromHours(1);

    private static readonly TimeSpan EnqueuedKeyRetention =
        TimeSpan.FromDays(2);

    private const int MaxOccurrencesPerEvaluation = 1000;

>>>>>>> c347f0b (Restore local project)
    private readonly IConfigurationService _configurationService;
    private readonly IScheduleService _scheduleService;
    private readonly IAutomationService _automationService;
    private readonly IStatusService _statusService;
<<<<<<< HEAD
    private readonly ILoggerService _logger;

    private CancellationTokenSource? _workerCts;
    private Task? _workerTask;
    private Task? _configWatchTask;
    private DateTimeOffset? _lastMissedExecutionCheck;
=======
    private readonly IRunQueueService _runQueue;
    private readonly ILoggerService _logger;

    private readonly ConcurrentDictionary<Guid, Task>
        _runningTasks = new();

    // Scheduler-thread state (only touched by the scheduler loop).
    private readonly Dictionary<string, ScheduleCursor>
        _cursors = new(StringComparer.OrdinalIgnoreCase);

    private readonly Dictionary<string, DateTimeOffset>
        _enqueuedKeys = new();

    private bool _firstEvaluation = true;
    private bool _schedulerHasError;
    private bool _statusInitialised;
    private DateTimeOffset? _reportedNextRun;
    private DateTimeOffset? _lastHistoryCleanup;

    private CancellationTokenSource? _workerCts;
    private Task? _workerTask;
    private Task? _runnerTask;
    private Task? _configWatchTask;
>>>>>>> c347f0b (Restore local project)

    public AutomationWorker(
        IConfigurationService configurationService,
        IScheduleService scheduleService,
        IAutomationService automationService,
        IStatusService statusService,
<<<<<<< HEAD
=======
        IRunQueueService runQueue,
>>>>>>> c347f0b (Restore local project)
        ILoggerService logger)
    {
        _configurationService = configurationService;
        _scheduleService = scheduleService;
        _automationService = automationService;
        _statusService = statusService;
<<<<<<< HEAD
=======
        _runQueue = runQueue;
>>>>>>> c347f0b (Restore local project)
        _logger = logger;
    }

    public bool IsRunning =>
        _workerTask is not null &&
        !_workerTask.IsCompleted;

    public async Task StartAsync(
        CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            return;
        }

        await _logger.InfoAsync(
            "Automation worker starting...",
            cancellationToken);

        await _scheduleService.LoadHistoryAsync(
            cancellationToken);

<<<<<<< HEAD
=======
        _cursors.Clear();
        _enqueuedKeys.Clear();
        _firstEvaluation = true;
        _schedulerHasError = false;
        _statusInitialised = false;
        _reportedNextRun = null;
        _lastHistoryCleanup = null;

        _runQueue.Clear();

>>>>>>> c347f0b (Restore local project)
        _workerCts =
            CancellationTokenSource
                .CreateLinkedTokenSource(
                    cancellationToken);

        _statusService.Update(status =>
        {
            status.State = WorkerState.Starting;
            status.StartedAt = DateTimeOffset.Now;
<<<<<<< HEAD
            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        _workerTask =
            Task.Run(
                () => WorkerLoopAsync(_workerCts.Token),
                _workerCts.Token);

        _configWatchTask =
            Task.Run(
                () => ConfigWatchLoopAsync(_workerCts.Token),
                _workerCts.Token);
=======
            status.RunningExecutions = 0;
            status.QueuedExecutions = 0;
            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        var token = _workerCts.Token;

        _workerTask =
            Task.Run(
                () => SchedulerLoopAsync(token),
                token);

        _runnerTask =
            Task.Run(
                () => RunnerLoopAsync(token),
                token);

        _configWatchTask =
            Task.Run(
                () => ConfigWatchLoopAsync(token),
                token);
>>>>>>> c347f0b (Restore local project)

        _statusService.Update(status =>
        {
            status.State = WorkerState.Running;
            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        await _logger.InfoAsync(
            "Automation worker started.",
            cancellationToken);
    }

    public async Task StopAsync()
    {
        if (_workerCts is null)
        {
            return;
        }

        await _logger.InfoAsync(
            "Automation worker stopping...");

        _statusService.Update(status =>
        {
            status.State = WorkerState.Stopping;
            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        _workerCts.Cancel();

<<<<<<< HEAD
        if (_workerTask is not null)
        {
            try
            {
                await _workerTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        if (_configWatchTask is not null)
        {
            try
            {
                await _configWatchTask;
            }
            catch (OperationCanceledException)
            {
            }
        }
=======
        await AwaitQuietlyAsync(_workerTask);
        await AwaitQuietlyAsync(_runnerTask);
        await AwaitQuietlyAsync(_configWatchTask);

        // Running executions are terminated by the cancellation
        // (application shutdown) and unwind on their own.
        await AwaitQuietlyAsync(
            Task.WhenAll(_runningTasks.Values.ToArray()));

        _runQueue.Clear();
>>>>>>> c347f0b (Restore local project)

        _statusService.Update(status =>
        {
            status.State = WorkerState.Stopped;
<<<<<<< HEAD
=======
            status.CurrentProcess = null;
            status.RunningExecutions = 0;
            status.QueuedExecutions = 0;
>>>>>>> c347f0b (Restore local project)
            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        await _logger.InfoAsync(
            "Automation worker stopped.");
    }

<<<<<<< HEAD
    private async Task WorkerLoopAsync(
=======
    /// <summary>
    /// Queues a manual run (Processes tab). It starts immediately
    /// when a slot is free, otherwise it waits in the Run Queue.
    /// </summary>
    public async Task EnqueueManualRunAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(processName))
        {
            throw new ArgumentException(
                "Process name is required.",
                nameof(processName));
        }

        if (!IsRunning)
        {
            await StartAsync(cancellationToken);
        }

        var config =
            _configurationService.Current;

        var process =
            config.Processes.FirstOrDefault(x =>
                string.Equals(
                    x.Name,
                    processName,
                    StringComparison.OrdinalIgnoreCase))
            ?? new ProcessConfig
            {
                Name = processName,
                FileName = $"{processName}.bat"
            };

        var scheduled = new ScheduledProcess
        {
            ProcessName = process.Name,
            Process = process,
            ScheduledTime = DateTimeOffset.Now
        };

        var item =
            _runQueue.Enqueue(
                scheduled,
                isManual: true,
                maxConcurrent: GetMaxConcurrent());

        if (item.IsVisible)
        {
            await _logger.WarningAsync(
                $"Manual run of {process.Name} conflicts with a running execution. Added to Run Queue.",
                cancellationToken);
        }
        else
        {
            await _logger.InfoAsync(
                $"Manual run requested: {process.Name}",
                cancellationToken);
        }

        StartAvailable();
    }

    // =========================================================
    // Scheduler: turns due schedules into queued executions
    // =========================================================

    private async Task SchedulerLoopAsync(
>>>>>>> c347f0b (Restore local project)
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var config =
                    _configurationService.Current;

                if (!config.Enabled)
                {
<<<<<<< HEAD
                    _statusService.Update(status =>
                    {
                        status.State =
                            WorkerState.Waiting;
                    });

                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        cancellationToken);

                    continue;
                }

                await CheckMissedExecutionsAsync(
                    config,
                    cancellationToken);

                await _scheduleService
                    .CleanupHistoryAsync(
                        cancellationToken);

                var next =
                    _scheduleService
                        .GetNextScheduledProcess(
                            config.Processes,
                            DateTimeOffset.Now);

                if (next is null)
                {
                    _statusService.Update(status =>
                    {
                        status.State =
                            WorkerState.Waiting;

                        status.NextScheduledRun =
                            null;

                        status.LastUpdatedAt =
                            DateTimeOffset.Now;
                    });

                    await Task.Delay(
                        TimeSpan.FromMinutes(1),
                        cancellationToken);

                    continue;
                }

                _statusService.Update(status =>
                {
                    status.State =
                        WorkerState.Waiting;

                    status.NextScheduledRun =
                        next.ScheduledTime;

                    status.LastUpdatedAt =
                        DateTimeOffset.Now;
                });

                var delay =
                    next.ScheduledTime -
                    DateTimeOffset.Now;

                if (delay > TimeSpan.Zero)
                {
                    await _logger.InfoAsync(
                        $"Next process: {next.ProcessName} at {next.ScheduledTime:yyyy-MM-dd HH:mm:ss}",
                        cancellationToken);

                    await Task.Delay(
                        delay,
                        cancellationToken);
                }

                if (_scheduleService.IsDuplicateExecution(
                        next))
                {
                    await _logger.WarningAsync(
                        $"Duplicate execution prevented for {next.ProcessName}",
                        cancellationToken);

                    continue;
                }

                await ExecuteScheduledProcessAsync(
                    next,
=======
                    ReportScheduler(null, null);
                }
                else
                {
                    await CleanupHistoryIfDueAsync(
                        cancellationToken);

                    await EvaluateSchedulesAsync(
                        config,
                        cancellationToken);

                    var next =
                        _scheduleService
                            .GetNextScheduledProcess(
                                config.Processes,
                                DateTimeOffset.Now);

                    ReportScheduler(
                        next?.ScheduledTime,
                        next?.ProcessName);
                }

                await Task.Delay(
                    SchedulerTickInterval,
>>>>>>> c347f0b (Restore local project)
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
<<<<<<< HEAD
=======
                _schedulerHasError = true;

>>>>>>> c347f0b (Restore local project)
                _statusService.Update(status =>
                {
                    status.State = WorkerState.Error;
                    status.LastError = ex.Message;
                    status.LastUpdatedAt =
                        DateTimeOffset.Now;
                });

<<<<<<< HEAD
                await _logger.ErrorAsync(
                    ex,
                    cancellationToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    cancellationToken);
            }
        }
=======
                await LogErrorSafeAsync(ex);

                if (!await DelayAsync(
                        TimeSpan.FromSeconds(30),
                        cancellationToken))
                {
                    break;
                }
            }
        }
    }

    private async Task EvaluateSchedulesAsync(
        AppConfig config,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        var active =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (var process in config.Processes.ToArray())
        {
            var schedule = process.Schedule;

            if (!process.Enabled ||
                schedule is null ||
                !schedule.Enabled)
            {
                _cursors.Remove(process.Name);

                continue;
            }

            active.Add(process.Name);

            var signature =
                BuildScheduleSignature(schedule);

            if (!_cursors.TryGetValue(
                    process.Name,
                    out var cursor) ||
                cursor.Signature != signature)
            {
                // First time this schedule is seen (application
                // start, or the schedule was created/edited).
                cursor =
                    new ScheduleCursor(
                        signature,
                        now - GetLookback(schedule));

                _cursors[process.Name] = cursor;
            }

            // Find the latest occurrence that became due since the
            // last evaluation. Several missed occurrences collapse
            // into a single execution.
            DateTimeOffset? latestDue = null;

            var from = cursor.LastEvaluated;

            for (var i = 0;
                 i < MaxOccurrencesPerEvaluation;
                 i++)
            {
                var next =
                    _scheduleService.GetNextRun(
                        schedule,
                        from);

                if (next is null ||
                    next.Value > now)
                {
                    break;
                }

                latestDue = next;

                from = next.Value;
            }

            cursor.LastEvaluated = now;

            if (latestDue is not null)
            {
                await TryEnqueueScheduledAsync(
                    process,
                    latestDue.Value,
                    cancellationToken);
            }
        }

        foreach (var stale in _cursors.Keys
                     .Where(x => !active.Contains(x))
                     .ToList())
        {
            _cursors.Remove(stale);
        }

        _firstEvaluation = false;
    }

    private TimeSpan GetLookback(
        ScheduleDefinition schedule)
    {
        if (!schedule.RunMissedExecution)
        {
            return TimeSpan.Zero;
        }

        var lookback =
            _firstEvaluation
                ? schedule.MaxLateExecutionWindow
                : NewScheduleGracePeriod;

        return lookback < TimeSpan.Zero
            ? TimeSpan.Zero
            : lookback;
    }

    private static string BuildScheduleSignature(
        ScheduleDefinition schedule)
    {
        return string.Join(
            "|",
            (int)schedule.Type,
            schedule.StartDateTime.UtcTicks,
            schedule.DayOfWeek?.ToString() ?? string.Empty,
            schedule.RunMissedExecution,
            schedule.MaxLateExecutionWindow.Ticks);
    }

    private async Task TryEnqueueScheduledAsync(
        ProcessConfig process,
        DateTimeOffset dueTime,
        CancellationToken cancellationToken)
    {
        var key =
            $"{process.Name}|{dueTime.UtcTicks}";

        if (_enqueuedKeys.ContainsKey(key))
        {
            return;
        }

        _enqueuedKeys[key] = DateTimeOffset.Now;

        var scheduled = new ScheduledProcess
        {
            ProcessName = process.Name,
            Process = process,
            ScheduledTime = dueTime
        };

        if (_scheduleService.IsDuplicateExecution(
                scheduled))
        {
            await _logger.WarningAsync(
                $"Duplicate execution prevented for {process.Name}",
                cancellationToken);

            return;
        }

        var item =
            _runQueue.Enqueue(
                scheduled,
                isManual: false,
                maxConcurrent: GetMaxConcurrent());

        if (item.IsVisible)
        {
            await _logger.WarningAsync(
                $"Scheduling conflict: {process.Name} was due at {dueTime:yyyy-MM-dd HH:mm:ss} while another execution is in progress. Added to Run Queue.",
                cancellationToken);
        }
        else
        {
            await _logger.InfoAsync(
                $"Scheduled execution due: {process.Name} ({dueTime:yyyy-MM-dd HH:mm:ss})",
                cancellationToken);
        }

        StartAvailable();
    }

    private async Task CleanupHistoryIfDueAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        if (_lastHistoryCleanup is not null &&
            now - _lastHistoryCleanup.Value <
            HistoryCleanupInterval)
        {
            return;
        }

        _lastHistoryCleanup = now;

        await _scheduleService.CleanupHistoryAsync(
            cancellationToken);

        foreach (var key in _enqueuedKeys
                     .Where(x =>
                         now - x.Value > EnqueuedKeyRetention)
                     .Select(x => x.Key)
                     .ToList())
        {
            _enqueuedKeys.Remove(key);
        }
    }

    /// <summary>
    /// Publishes the next scheduled run to the status snapshot,
    /// only touching status.json when something changed.
    /// </summary>
    private void ReportScheduler(
        DateTimeOffset? nextRun,
        string? nextProcessName)
    {
        if (_statusInitialised &&
            !_schedulerHasError &&
            _reportedNextRun == nextRun)
        {
            return;
        }

        var changedRun =
            !_statusInitialised ||
            _reportedNextRun != nextRun;

        _statusInitialised = true;
        _schedulerHasError = false;
        _reportedNextRun = nextRun;

        var running = _runQueue.RunningCount;

        _statusService.Update(status =>
        {
            status.NextScheduledRun = nextRun;

            var idleState =
                status.State == WorkerState.Running ||
                status.State == WorkerState.Starting ||
                status.State == WorkerState.Error;

            if (idleState && running == 0)
            {
                status.State = WorkerState.Waiting;
            }

            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        if (changedRun &&
            nextRun is not null &&
            nextProcessName is not null)
        {
            _ = LogInfoSafeAsync(
                $"Next process: {nextProcessName} at {nextRun.Value:yyyy-MM-dd HH:mm:ss}");
        }
    }

    // =========================================================
    // Runner: starts queued executions when a slot is free
    // =========================================================

    private async Task RunnerLoopAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                StartAvailable();

                await Task.Delay(
                    RunnerTickInterval,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                await LogErrorSafeAsync(ex);

                if (!await DelayAsync(
                        TimeSpan.FromSeconds(5),
                        cancellationToken))
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Starts as many queued executions as the concurrency limit
    /// allows. Safe to call from any thread at any time.
    /// </summary>
    private void StartAvailable()
    {
        var token =
            _workerCts?.Token ??
            CancellationToken.None;

        if (token.IsCancellationRequested)
        {
            return;
        }

        var maxConcurrent = GetMaxConcurrent();

        while (true)
        {
            var item =
                _runQueue.TryStartNext(
                    maxConcurrent,
                    DateTimeOffset.Now);

            if (item is null)
            {
                break;
            }

            var started = item;

            var task =
                Task.Run(
                    () => ExecuteQueueItemAsync(
                        started,
                        token));

            _runningTasks[started.Id] = task;

            _ = task.ContinueWith(
                _ =>
                {
                    _runningTasks.TryRemove(
                        started.Id,
                        out _);
                },
                TaskScheduler.Default);
        }
    }

    private async Task ExecuteQueueItemAsync(
        RunQueueItem item,
        CancellationToken cancellationToken)
    {
        var requeued = false;

        try
        {
            RefreshActivityStatus();

            _runQueue.AppendLog(
                item.Id,
                item.Attempts > 1
                    ? $"Attempt {item.Attempts}: starting {item.ProcessName}."
                    : $"Starting {item.ProcessName}.");

            await LogInfoSafeAsync(
                $"Executing process: {item.ProcessName}");

            var result =
                await _automationService
                    .ExecuteAsync(
                        item.ProcessName,
                        line => _runQueue.AppendLog(
                            item.Id,
                            line),
                        cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                _runQueue.AppendLog(
                    item.Id,
                    "Execution cancelled (application shutdown).");

                return;
            }

            if (result.IsConflict)
            {
                // Another UiPath foreground process is running.
                // Do not fail or skip: wait in the Run Queue and
                // try again shortly.
                var retryAt =
                    DateTimeOffset.Now +
                    ConflictRetryDelay;

                _runQueue.Requeue(
                    item.Id,
                    retryAt,
                    $"Conflict: another UiPath foreground process is running. Retrying at {retryAt:HH:mm:ss}.");

                requeued = true;

                await LogWarningSafeAsync(
                    $"Process blocked by a running UiPath foreground process: {item.ProcessName}. Kept in Run Queue, retrying in {ConflictRetryDelay.TotalSeconds:0} second(s).");

                return;
            }

            _runQueue.AppendLog(
                item.Id,
                result.Success
                    ? "Completed successfully."
                    : $"Failed: {result.Error}");

            if (!item.IsManual)
            {
                await _scheduleService.MarkExecutionAsync(
                    new ScheduledProcess
                    {
                        ProcessName = item.ProcessName,
                        Process = item.Process,
                        ScheduledTime = item.ScheduledTime
                    },
                    cancellationToken);
            }

            _statusService.Update(status =>
            {
                status.LastExecutionTime =
                    result.FinishedAt;

                status.LastResult = result;

                if (result.Success)
                {
                    status.SuccessfulExecutions++;
                }
                else
                {
                    status.FailedExecutions++;

                    status.LastError =
                        result.Error;
                }

                status.LastUpdatedAt =
                    DateTimeOffset.Now;
            });

            if (result.Success)
            {
                await LogInfoSafeAsync(
                    $"Process completed: {item.ProcessName}");
            }
            else
            {
                await LogErrorTextSafeAsync(
                    $"Process failed: {item.ProcessName} | {result.Error}");
            }
        }
        catch (OperationCanceledException)
        {
            // Application shutdown.
        }
        catch (Exception ex)
        {
            _runQueue.AppendLog(
                item.Id,
                $"Unexpected error: {ex.Message}");

            _statusService.Update(status =>
            {
                status.FailedExecutions++;
                status.LastError = ex.Message;
                status.LastUpdatedAt =
                    DateTimeOffset.Now;
            });

            await LogErrorSafeAsync(ex);
        }
        finally
        {
            if (!requeued)
            {
                _runQueue.Complete(item.Id);
            }

            RefreshActivityStatus();

            // A slot just became free: start whatever is waiting.
            StartAvailable();
        }
    }

    /// <summary>
    /// Publishes the running / queued counters and the current
    /// task to the status snapshot.
    /// </summary>
    private void RefreshActivityStatus()
    {
        var running =
            _runQueue.GetRunningProcessNames();

        var queued =
            _runQueue.QueuedCount;

        _statusService.Update(status =>
        {
            status.RunningExecutions = running.Count;
            status.QueuedExecutions = queued;

            if (status.State == WorkerState.Stopping ||
                status.State == WorkerState.Stopped)
            {
                return;
            }

            status.CurrentProcess =
                running.Count == 0
                    ? null
                    : string.Join(", ", running);

            status.State =
                running.Count > 0
                    ? WorkerState.Executing
                    : WorkerState.Waiting;

            status.LastUpdatedAt =
                DateTimeOffset.Now;
        });
    }

    private int GetMaxConcurrent()
    {
        return Math.Max(
            1,
            _configurationService.Current
                .MaxConcurrentTasks);
>>>>>>> c347f0b (Restore local project)
    }

    /// <summary>
    /// Config.json is the app's "memory" — the backend must
    /// notice edits made to it (new/changed schedules, toggled
    /// processes, etc.) without requiring a restart. This loop
    /// re-reads appsettings.json from the fixed Desktop Config
    /// folder every 30 seconds for the lifetime of the worker.
    /// </summary>
    private async Task ConfigWatchLoopAsync(
        CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(
                    ConfigRecheckInterval,
                    cancellationToken);

                await _configurationService.ReloadAsync(
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
<<<<<<< HEAD
                await _logger.ErrorAsync(
                    ex,
                    cancellationToken);
=======
                await LogErrorSafeAsync(ex);
>>>>>>> c347f0b (Restore local project)
            }
        }
    }

<<<<<<< HEAD
    private async Task CheckMissedExecutionsAsync(
        AppConfig config,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.Now;

        foreach (var process in config.Processes)
        {
            if (!process.Enabled ||
                !process.Schedule.Enabled ||
                !process.Schedule.RunMissedExecution)
            {
                continue;
            }

            var lastCheckTime =
                _lastMissedExecutionCheck ??
                now - process.Schedule.MaxLateExecutionWindow;

            if (!_scheduleService.IsMissedExecution(
                    process.Schedule,
                    lastCheckTime,
                    now))
            {
                continue;
            }

            var missedRun = new ScheduledProcess
            {
                ProcessName = process.Name,
                Process = process,
                ScheduledTime = now
            };

            if (_scheduleService.IsDuplicateExecution(
                    missedRun))
            {
                continue;
            }

            await _logger.WarningAsync(
                $"Missed execution detected for {process.Name}; running now.",
                cancellationToken);

            await ExecuteScheduledProcessAsync(
                missedRun,
                cancellationToken);
        }

        _lastMissedExecutionCheck = now;
    }

    private async Task ExecuteScheduledProcessAsync(
        ScheduledProcess scheduledProcess,
        CancellationToken cancellationToken)
    {
        var processName = scheduledProcess.ProcessName;
        var scheduledTime = scheduledProcess.ScheduledTime;

        await _logger.InfoAsync(
            $"Executing process: {processName}",
            cancellationToken);

        _statusService.Update(status =>
        {
            status.State =
                WorkerState.Executing;

            status.CurrentProcess =
                processName;

            status.LastUpdatedAt =
                DateTimeOffset.Now;
        });

        var result =
            await _automationService
                .ExecuteAsync(
                    processName,
                    cancellationToken);

        await _scheduleService.MarkExecutionAsync(
            scheduledProcess,
            cancellationToken);

        _statusService.Update(status =>
        {
            status.LastExecutionTime =
                result.FinishedAt;

            status.LastResult = result;

            status.CurrentProcess = null;

            status.State =
                WorkerState.Waiting;

            status.LastUpdatedAt =
                DateTimeOffset.Now;

            if (result.Success)
            {
                status.SuccessfulExecutions++;
            }
            else
            {
                status.FailedExecutions++;
                status.LastError =
                    result.Error;
            }
        });

        if (result.Success)
        {
            await _logger.InfoAsync(
                $"Process completed: {processName}",
                cancellationToken);
        }
        else
        {
            await _logger.ErrorAsync(
                $"Process failed: {processName} | {result.Error}",
                cancellationToken);
        }
    }
}
=======
    // =========================================================
    // Helpers
    // =========================================================

    private static async Task<bool> DelayAsync(
        TimeSpan delay,
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(
                delay,
                cancellationToken);

            return true;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    private static async Task AwaitQuietlyAsync(
        Task? task)
    {
        if (task is null)
        {
            return;
        }

        try
        {
            await task;
        }
        catch
        {
        }
    }

    private async Task LogInfoSafeAsync(
        string message)
    {
        try
        {
            await _logger.InfoAsync(message);
        }
        catch
        {
        }
    }

    private async Task LogWarningSafeAsync(
        string message)
    {
        try
        {
            await _logger.WarningAsync(message);
        }
        catch
        {
        }
    }

    private async Task LogErrorTextSafeAsync(
        string message)
    {
        try
        {
            await _logger.ErrorAsync(message);
        }
        catch
        {
        }
    }

    private async Task LogErrorSafeAsync(
        Exception exception)
    {
        try
        {
            await _logger.ErrorAsync(exception);
        }
        catch
        {
        }
    }

    private sealed class ScheduleCursor
    {
        public ScheduleCursor(
            string signature,
            DateTimeOffset lastEvaluated)
        {
            Signature = signature;
            LastEvaluated = lastEvaluated;
        }

        public string Signature { get; }

        public DateTimeOffset LastEvaluated { get; set; }
    }
}
>>>>>>> c347f0b (Restore local project)
