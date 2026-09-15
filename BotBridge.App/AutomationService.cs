using System.Diagnostics;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Services;

public sealed class AutomationService : IAutomationService
{
    private readonly IProcessService _processService;
    private readonly IConfigurationService _configurationService;

    public AutomationService(
        IProcessService processService,
        IConfigurationService configurationService)
    {
        _processService = processService;
        _configurationService = configurationService;
    }

    public async Task<TaskExecutionResult> ExecuteAsync(
        string processName,
        CancellationToken cancellationToken = default)
    {
        var executionId = Guid.NewGuid();

        var startedAt = DateTimeOffset.Now;

        try
        {
            await _processService.ValidateProcessAsync(
                processName,
                cancellationToken);

            var processPath =
                await _processService.GetProcessPathAsync(
                    processName,
                    cancellationToken);

            if (!File.Exists(processPath))
            {
                return CreateFailure(
                    executionId,
                    processName,
                    startedAt,
                    $"Process file not found: {processPath}");
            }

            var processConfig =
                _configurationService.Current.Processes
                    .FirstOrDefault(x =>
                        x.Name == processName);

            var timeoutMinutes =
                processConfig?.TimeoutMinutes
                    ?? _configurationService.Current
                        .ProcessTimeoutMinutes;

            var timeout =
                TimeSpan.FromMinutes(timeoutMinutes);

            using var process = new Process();

            process.StartInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{processPath}\"",
                WorkingDirectory =
                    Path.GetDirectoryName(processPath),

                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            process.Start();

            var processId = process.Id;

            var outputTask =
                process.StandardOutput.ReadToEndAsync();

            var errorTask =
                process.StandardError.ReadToEndAsync();

            using var timeoutCts =
                new CancellationTokenSource(timeout);

            using var linkedCts =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken,
                    timeoutCts.Token);

            try
            {
                await process.WaitForExitAsync(
                    linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(true);
                    }
                }
                catch
                {
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return CreateFailure(
                        executionId,
                        processName,
                        startedAt,
                        "Process execution cancelled.");
                }

                return CreateFailure(
                    executionId,
                    processName,
                    startedAt,
                    $"Process exceeded timeout of {timeout.TotalMinutes} minute(s).");
            }

            var output = await outputTask;
            var error = await errorTask;

            var finishedAt =
                DateTimeOffset.Now;

            if (process.ExitCode != 0)
            {
                return new TaskExecutionResult
                {
                    ExecutionId = executionId,
                    ProcessName = processName,
                    Success = false,
                    ProcessId = processId,
                    StartedAt = startedAt,
                    FinishedAt = finishedAt,
                    Message = "Process failed.",
                    Error =
                        string.IsNullOrWhiteSpace(error)
                            ? $"Exit Code: {process.ExitCode}"
                            : error.Trim()
                };
            }

            return new TaskExecutionResult
            {
                ExecutionId = executionId,
                ProcessName = processName,
                Success = true,
                ProcessId = processId,
                StartedAt = startedAt,
                FinishedAt = finishedAt,
                Message =
                    string.IsNullOrWhiteSpace(output)
                        ? "Process completed successfully."
                        : output.Trim(),
                Error = null
            };
        }
        catch (Exception ex)
        {
            return CreateFailure(
                executionId,
                processName,
                startedAt,
                ex.Message);
        }
    }

    private static TaskExecutionResult CreateFailure(
        Guid executionId,
        string processName,
        DateTimeOffset startedAt,
        string error)
    {
        return new TaskExecutionResult
        {
            ExecutionId = executionId,
            ProcessName = processName,
            Success = false,
            StartedAt = startedAt,
            FinishedAt = DateTimeOffset.Now,
            Message = "Process failed.",
            Error = error
        };
    }
}