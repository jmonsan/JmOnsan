using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Workers;

public sealed class AutomationWorker
{
    private readonly IConfigurationService _configurationService;
    private readonly IScheduleService _scheduleService;
    private readonly IAutomationService _automationService;
    private readonly IStatusService _statusService;
    private readonly ILoggerService _logger;

    private CancellationTokenSource? _workerCts;
    private Task? _workerTask;

    public AutomationWorker(
        IConfigurationService configurationService,
        IScheduleService scheduleService,
        IAutomationService automationService,
        IStatusService statusService,
        ILoggerService logger)
    {
        _configurationService = configurationService;
        _scheduleService = scheduleService;
        _automationService = automationService;
        _statusService = statusService;
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

        _workerCts =
            CancellationTokenSource
                .CreateLinkedTokenSource(
                    cancellationToken);

        _statusService.Update(status =>
        {
            status.State = WorkerState.Starting;
            status.StartedAt = DateTimeOffset.Now;
            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        _workerTask =
            Task.Run(
                () => WorkerLoopAsync(_workerCts.Token),
                _workerCts.Token);

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

        _statusService.Update(status =>
        {
            status.State = WorkerState.Stopped;
            status.LastUpdatedAt = DateTimeOffset.Now;
        });

        await _logger.InfoAsync(
            "Automation worker stopped.");
    }

    private async Task WorkerLoopAsync(
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
                        next.ProcessName,
                        next.ScheduledTime))
                {
                    await _logger.WarningAsync(
                        $"Duplicate execution prevented for {next.ProcessName}",
                        cancellationToken);

                    continue;
                }

                await ExecuteScheduledProcessAsync(
                    next.ProcessName,
                    next.ScheduledTime,
                    cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _statusService.Update(status =>
                {
                    status.State = WorkerState.Error;
                    status.LastError = ex.Message;
                    status.LastUpdatedAt =
                        DateTimeOffset.Now;
                });

                await _logger.ErrorAsync(
                    ex,
                    cancellationToken);

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    cancellationToken);
            }
        }
    }

    private async Task ExecuteScheduledProcessAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken)
    {
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
            processName,
            scheduledTime,
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