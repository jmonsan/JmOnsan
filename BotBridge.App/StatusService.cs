using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.Application.Services;

public sealed class StatusService : IStatusService
{
    private readonly object _syncRoot = new();

    public BackendStatus Current { get; } =
        new();

    public event EventHandler<BackendStatus>?
        StatusChanged;

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

            Current.StartedAt =
                null;

            Current.LastUpdatedAt =
                DateTimeOffset.Now;

            snapshot = CreateSnapshot();
        }

        StatusChanged?.Invoke(
            this,
            snapshot);
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

            StartedAt =
                Current.StartedAt,

            LastUpdatedAt =
                Current.LastUpdatedAt
        };
    }
}