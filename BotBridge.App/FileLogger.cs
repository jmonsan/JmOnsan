using System.Text;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Infrastructure.Logging;

public sealed class FileLogger : ILoggerService
{
    private readonly string _logsFolder;

<<<<<<< HEAD
=======
    // The worker, the scheduler and running executions all log
    // concurrently; writes to the day file are serialized.
    private readonly SemaphoreSlim _writeGate = new(1, 1);

>>>>>>> c347f0b (Restore local project)
    public FileLogger(string logsFolder)
    {
        _logsFolder = logsFolder;

        Directory.CreateDirectory(_logsFolder);
    }

    public Task InfoAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            "INFO",
            message,
            cancellationToken);
    }

    public Task WarningAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            "WARN",
            message,
            cancellationToken);
    }

    public Task ErrorAsync(
        string message,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            "ERROR",
            message,
            cancellationToken);
    }

    public Task ErrorAsync(
        Exception exception,
        CancellationToken cancellationToken = default)
    {
        return WriteAsync(
            "ERROR",
            exception.ToString(),
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<LogEntry>>
        GetRecentLogsAsync(
            int count = 100,
            CancellationToken cancellationToken = default)
    {
        var logFile =
            Path.Combine(
                _logsFolder,
                $"botbridge-{DateTime.Now:yyyy-MM-dd}.log");

        if (!File.Exists(logFile))
        {
            return Array.Empty<LogEntry>();
        }

<<<<<<< HEAD
        var lines =
            await File.ReadAllLinesAsync(
                logFile,
                cancellationToken);
=======
        string[] lines;

        await _writeGate.WaitAsync(cancellationToken);

        try
        {
            lines =
                await File.ReadAllLinesAsync(
                    logFile,
                    cancellationToken);
        }
        finally
        {
            _writeGate.Release();
        }
>>>>>>> c347f0b (Restore local project)

        return lines
            .TakeLast(count)
            .Select(ParseLogLine)
            .ToList();
    }

    private static LogEntry ParseLogLine(
        string line)
    {
        var level = "INFO";

        if (line.Contains("[ERROR]"))
        {
            level = "ERROR";
        }
        else if (line.Contains("[WARN]"))
        {
            level = "WARN";
        }

        return new LogEntry
        {
            Timestamp = DateTimeOffset.Now,
            Level = level,
            Message = line
        };
    }

    private async Task WriteAsync(
        string level,
        string message,
        CancellationToken cancellationToken)
    {
        var filePath =
            Path.Combine(
                _logsFolder,
                $"botbridge-{DateTime.Now:yyyy-MM-dd}.log");

        var line =
            $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";

<<<<<<< HEAD
        await File.AppendAllTextAsync(
            filePath,
            line + Environment.NewLine,
            Encoding.UTF8,
            cancellationToken);
=======
        await _writeGate.WaitAsync(cancellationToken);

        try
        {
            for (var attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    await File.AppendAllTextAsync(
                        filePath,
                        line + Environment.NewLine,
                        Encoding.UTF8,
                        cancellationToken);

                    return;
                }
                catch (Exception ex) when (
                    ex is IOException or
                    UnauthorizedAccessException)
                {
                    // A log viewer may briefly hold the file open.
                    // Retry a couple of times; logging must never
                    // take the worker down.
                    if (attempt == 2)
                    {
                        return;
                    }

                    await Task.Delay(
                        50,
                        cancellationToken);
                }
            }
        }
        finally
        {
            _writeGate.Release();
        }
>>>>>>> c347f0b (Restore local project)
    }
}