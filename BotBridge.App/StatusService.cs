using System.Text.Json;
using System.Text.Json.Serialization;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Services;

public sealed class StatusService : IStatusService
{
    private readonly object _syncRoot = new();

    private readonly string? _statusFilePath;

    private static readonly JsonSerializerOptions
        JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy =
                JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

    public BackendStatus Current { get; } =
        new();

    public event EventHandler<BackendStatus>?
        StatusChanged;

    public StatusService()
        : this(statusFilePath: null)
    {
    }

    public StatusService(string? statusFilePath)
    {
        _statusFilePath = statusFilePath;

        RestoreFromDisk();
    }

    public void Update(
        Action<BackendStatus> updateAction)
    {
        ArgumentNullException.ThrowIfNull(
            updateAction);

        BackendStatus snapshot;

        lock (_syncRoot)
        {
            updateAction(Current);

            Current.LastUpdatedAt =
                DateTimeOffset.Now;

            snapshot = CreateSnapshot();

            PersistToDisk(snapshot);
        }

        StatusChanged?.Invoke(
            this,
            snapshot);
    }

    public BackendStatus GetSnapshot()
    {
        lock (_syncRoot)
        {
            return CreateSnapshot();
        }
    }

    public void Reset()
    {
        BackendStatus snapshot;

        lock (_syncRoot)
        {
            Current.State =
                WorkerState.Stopped;

            Current.CurrentProcess =
                null;

            Current.CurrentExecutionId =
                null;

            Current.NextScheduledRun =
                null;

            Current.LastExecutionTime =
                null;

            Current.LastResult =
                null;

            Current.LastError =
                null;

            Current.SuccessfulExecutions =
                0;

            Current.FailedExecutions =
                0;

<<<<<<< HEAD
=======
            Current.RunningExecutions =
                0;

            Current.QueuedExecutions =
                0;

>>>>>>> c347f0b (Restore local project)
            Current.StartedAt =
                null;

            Current.LastUpdatedAt =
                DateTimeOffset.Now;

            snapshot = CreateSnapshot();

            PersistToDisk(snapshot);
        }

        StatusChanged?.Invoke(
            this,
            snapshot);
    }

    /// <summary>
    /// Loads status.json (if present) so counters and last
    /// execution info survive an app restart. The live
    /// worker state is intentionally reset to Stopped, since
    /// a persisted "Running"/"Executing" state would be
    /// stale the moment the process restarts.
    /// </summary>
    private void RestoreFromDisk()
    {
        if (string.IsNullOrWhiteSpace(_statusFilePath) ||
            !File.Exists(_statusFilePath))
        {
            return;
        }

        try
        {
            var json =
                File.ReadAllText(_statusFilePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return;
            }

            var restored =
                JsonSerializer.Deserialize<BackendStatus>(
                    json,
                    JsonOptions);

            if (restored is null)
            {
                return;
            }

            Current.LastExecutionTime =
                restored.LastExecutionTime;

            Current.LastResult =
                restored.LastResult;

            Current.LastError =
                restored.LastError;

            Current.SuccessfulExecutions =
                restored.SuccessfulExecutions;

            Current.FailedExecutions =
                restored.FailedExecutions;

            // Live/session-only fields stay at their
            // freshly-constructed defaults: State (Stopped),
            // CurrentProcess, CurrentExecutionId,
<<<<<<< HEAD
            // NextScheduledRun, StartedAt.
=======
            // NextScheduledRun, RunningExecutions,
            // QueuedExecutions, StartedAt.
>>>>>>> c347f0b (Restore local project)
            Current.LastUpdatedAt =
                DateTimeOffset.Now;
        }
        catch (JsonException)
        {
            // Corrupt/partial status.json shouldn't prevent
            // startup; fall back to a clean default status.
        }
    }

    private void PersistToDisk(BackendStatus snapshot)
    {
        if (string.IsNullOrWhiteSpace(_statusFilePath))
        {
            return;
        }

        try
        {
            var directory =
                Path.GetDirectoryName(_statusFilePath);

            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json =
                JsonSerializer.Serialize(
                    snapshot,
                    JsonOptions);

            File.WriteAllText(
                _statusFilePath,
                json);
        }
        catch (IOException)
        {
            // Best-effort persistence; a transient file lock
            // shouldn't crash the worker loop.
        }
    }

    private BackendStatus CreateSnapshot()
    {
        return new BackendStatus
        {
            State =
                Current.State,

            CurrentProcess =
                Current.CurrentProcess,

            CurrentExecutionId =
                Current.CurrentExecutionId,

            NextScheduledRun =
                Current.NextScheduledRun,

            LastExecutionTime =
                Current.LastExecutionTime,

            LastResult =
                Current.LastResult,

            LastError =
                Current.LastError,

            SuccessfulExecutions =
                Current.SuccessfulExecutions,

            FailedExecutions =
                Current.FailedExecutions,

<<<<<<< HEAD
=======
            RunningExecutions =
                Current.RunningExecutions,

            QueuedExecutions =
                Current.QueuedExecutions,

>>>>>>> c347f0b (Restore local project)
            StartedAt =
                Current.StartedAt,

            LastUpdatedAt =
                Current.LastUpdatedAt
        };
    }
}