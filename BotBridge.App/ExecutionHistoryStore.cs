using System.Text.Json;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Infrastructure.Persistence;

public sealed class ExecutionHistoryStore
    : IExecutionHistoryStore
{
<<<<<<< HEAD
    private readonly string _filePath;

=======
    private static readonly JsonSerializerOptions
        WriteOptions = new()
        {
            WriteIndented = true
        };

    private readonly string _filePath;

    // Executions can now finish concurrently (Run Queue), so
    // every read-modify-write of the history file is serialized.
    private readonly SemaphoreSlim _gate = new(1, 1);

>>>>>>> c347f0b (Restore local project)
    public ExecutionHistoryStore(string filePath)
    {
        _filePath = filePath;

        var directory =
            Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public async Task<bool> IsExecutedAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
    {
<<<<<<< HEAD
        var records =
            await GetAllAsync(cancellationToken);

        return records.Any(x =>
            x.ProcessName == processName &&
            x.ScheduledTime == scheduledTime);
=======
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var records =
                await ReadAsync(cancellationToken);

            return records.Any(x =>
                x.ProcessName == processName &&
                x.ScheduledTime == scheduledTime);
        }
        finally
        {
            _gate.Release();
        }
>>>>>>> c347f0b (Restore local project)
    }

    public async Task MarkExecutedAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
    {
<<<<<<< HEAD
        var records =
            (await GetAllAsync(cancellationToken))
            .ToList();

        records.Add(
            new ExecutionHistoryEntry
            {
                ProcessName = processName,
                ScheduledTime = scheduledTime,
                ExecutedAt = DateTimeOffset.Now
            });

        var json =
            JsonSerializer.Serialize(
                records,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });

        await File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken);
=======
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var records =
                (await ReadAsync(cancellationToken))
                .ToList();

            records.Add(
                new ExecutionHistoryEntry
                {
                    ProcessName = processName,
                    ScheduledTime = scheduledTime,
                    ExecutedAt = DateTimeOffset.Now
                });

            await WriteAsync(
                records,
                cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
>>>>>>> c347f0b (Restore local project)
    }

    public async Task<IReadOnlyCollection<ExecutionHistoryEntry>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
<<<<<<< HEAD
=======
        await _gate.WaitAsync(cancellationToken);

        try
        {
            return await ReadAsync(cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task CleanupAsync(
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);

        try
        {
            var cutoff =
                DateTimeOffset.Now.Subtract(retention);

            var all =
                await ReadAsync(cancellationToken);

            var records =
                all
                    .Where(x => x.ExecutedAt >= cutoff)
                    .ToList();

            // Nothing expired: avoid rewriting the file.
            if (records.Count == all.Count)
            {
                return;
            }

            await WriteAsync(
                records,
                cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IReadOnlyCollection<ExecutionHistoryEntry>>
        ReadAsync(
            CancellationToken cancellationToken)
    {
>>>>>>> c347f0b (Restore local project)
        if (!File.Exists(_filePath))
        {
            return [];
        }

        var json =
            await File.ReadAllTextAsync(
                _filePath,
                cancellationToken);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<
            List<ExecutionHistoryEntry>>(json)
            ?? [];
    }

<<<<<<< HEAD
    public async Task CleanupAsync(
        TimeSpan retention,
        CancellationToken cancellationToken = default)
    {
        var cutoff =
            DateTimeOffset.Now.Subtract(retention);

        var records =
            (await GetAllAsync(cancellationToken))
            .Where(x => x.ExecutedAt >= cutoff)
            .ToList();

        var json =
            JsonSerializer.Serialize(
                records,
                new JsonSerializerOptions
                {
                    WriteIndented = true
                });
=======
    private async Task WriteAsync(
        IReadOnlyCollection<ExecutionHistoryEntry> records,
        CancellationToken cancellationToken)
    {
        var json =
            JsonSerializer.Serialize(
                records,
                WriteOptions);
>>>>>>> c347f0b (Restore local project)

        await File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken);
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> c347f0b (Restore local project)
