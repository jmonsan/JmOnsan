using System.Text.Json;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Infrastructure.Persistence;

public sealed class ExecutionHistoryStore
    : IExecutionHistoryStore
{
    private readonly string _filePath;

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
        var records =
            await GetAllAsync(cancellationToken);

        return records.Any(x =>
            x.ProcessName == processName &&
            x.ScheduledTime == scheduledTime);
    }

    public async Task MarkExecutedAsync(
        string processName,
        DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
    {
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
    }

    public async Task<IReadOnlyCollection<ExecutionHistoryEntry>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
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

        await File.WriteAllTextAsync(
            _filePath,
            json,
            cancellationToken);
    }
}