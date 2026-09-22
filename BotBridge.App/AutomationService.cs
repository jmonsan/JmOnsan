using System.Diagnostics;
<<<<<<< HEAD
=======
using System.Text;
>>>>>>> c347f0b (Restore local project)
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Services;

public sealed class AutomationService : IAutomationService
{
<<<<<<< HEAD
    private readonly IProcessService _processService;
    private readonly IConfigurationService _configurationService;

    public AutomationService(
        IProcessService processService,
        IConfigurationService configurationService)
    {
        _processService = processService;
        _configurationService = configurationService;
=======
    // UiPath refuses to start a second foreground process while
    // another one is running. That is a scheduling conflict, not a
    // failure of the automation itself.
    private static readonly string[] ForegroundConflictMarkers =
    [
        "foreground process is already running",
        "only one foreground process can run"
    ];

    private readonly IProcessService _processService;

    public AutomationService(
        IProcessService processService)
    {
        _processService = processService;
>>>>>>> c347f0b (Restore local project)
    }

    public async Task<TaskExecutionResult> ExecuteAsync(
        string processName,
<<<<<<< HEAD
=======
        Action<string>? onOutput = null,
>>>>>>> c347f0b (Restore local project)
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

<<<<<<< HEAD
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

=======
>>>>>>> c347f0b (Restore local project)
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

<<<<<<< HEAD
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
=======
            var outputBuffer = new StringBuilder();
            var errorBuffer = new StringBuilder();

            var outputTask =
                PumpAsync(
                    process.StandardOutput,
                    outputBuffer,
                    onOutput,
                    string.Empty);

            var errorTask =
                PumpAsync(
                    process.StandardError,
                    errorBuffer,
                    onOutput,
                    "[stderr] ");

            try
            {
                // No runtime limit: wait for the process to end on
                // its own. Only application shutdown (cancellation)
                // ever terminates it.
                await process.WaitForExitAsync(
                    cancellationToken);
>>>>>>> c347f0b (Restore local project)
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

<<<<<<< HEAD
                if (cancellationToken.IsCancellationRequested)
                {
                    return CreateFailure(
                        executionId,
                        processName,
                        startedAt,
                        "Process execution cancelled.");
                }

=======
>>>>>>> c347f0b (Restore local project)
                return CreateFailure(
                    executionId,
                    processName,
                    startedAt,
<<<<<<< HEAD
                    $"Process exceeded timeout of {timeout.TotalMinutes} minute(s).");
            }

            var output = await outputTask;
            var error = await errorTask;
=======
                    "Process execution cancelled.");
            }

            await Task.WhenAll(
                outputTask,
                errorTask);

            var output = outputBuffer.ToString();
            var error = errorBuffer.ToString();
>>>>>>> c347f0b (Restore local project)

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
<<<<<<< HEAD
                            : error.Trim()
=======
                            : error.Trim(),
                    IsConflict =
                        IsForegroundConflict(error) ||
                        IsForegroundConflict(output)
>>>>>>> c347f0b (Restore local project)
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

<<<<<<< HEAD
=======
    private static bool IsForegroundConflict(
        string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        foreach (var marker in ForegroundConflictMarkers)
        {
            if (text.Contains(
                    marker,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Reads a redirected stream line by line, collecting the
    /// full text and forwarding each line to the callback.
    /// </summary>
    private static async Task PumpAsync(
        StreamReader reader,
        StringBuilder buffer,
        Action<string>? onLine,
        string prefix)
    {
        try
        {
            string? line;

            while ((line = await reader.ReadLineAsync()) is not null)
            {
                buffer.AppendLine(line);

                if (onLine is null)
                {
                    continue;
                }

                try
                {
                    onLine(prefix + line);
                }
                catch
                {
                    // A faulty callback must not stop the process
                    // output from being drained.
                }
            }
        }
        catch
        {
            // The stream is closed when the process is killed
            // or disposed; there is nothing left to read.
        }
    }

>>>>>>> c347f0b (Restore local project)
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
<<<<<<< HEAD
}
=======
}
>>>>>>> c347f0b (Restore local project)
