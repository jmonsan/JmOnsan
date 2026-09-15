using System.Text;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Infrastructure.Logging;

public sealed class FileLogger : ILoggerService
{
    private readonly string _logsFolder;

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

        var lines =
            await File.ReadAllLinesAsync(
                logFile,
                cancellationToken);

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

        await File.AppendAllTextAsync(
            filePath,
            line + Environment.NewLine,
            Encoding.UTF8,
            cancellationToken);
    }
}