namespace BotBridge.Core.Models;

public sealed class BackendStatus
{
    /// <summary>
    /// Current worker state.
    /// </summary>
    public WorkerState State { get; set; }
        = WorkerState.Stopped;

    /// <summary>
    /// Indicates whether the worker is active.
    /// </summary>
    public bool IsRunning =>
        State != WorkerState.Stopped &&
        State != WorkerState.Stopping;

    /// <summary>
    /// Current process being executed.
    /// Example: AWIR.bat
    /// </summary>
    public string? CurrentProcess { get; set; }

    /// <summary>
    /// Currently executing task identifier.
    /// </summary>
    public Guid? CurrentExecutionId { get; set; }

    /// <summary>
    /// Next scheduled execution time.
    /// </summary>
    public DateTimeOffset? NextScheduledRun { get; set; }

    /// <summary>
    /// Last successful execution time.
    /// </summary>
    public DateTimeOffset? LastExecutionTime { get; set; }

    /// <summary>
    /// Last completed task result.
    /// </summary>
    public TaskExecutionResult? LastResult { get; set; }

    /// <summary>
    /// Last error encountered by the worker.
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// Total successful executions.
    /// </summary>
    public long SuccessfulExecutions { get; set; }

    /// <summary>
    /// Total failed executions.
    /// </summary>
    public long FailedExecutions { get; set; }

    /// <summary>
<<<<<<< HEAD
=======
    /// Number of executions running right now.
    /// </summary>
    public int RunningExecutions { get; set; }

    /// <summary>
    /// Number of executions waiting in the Run Queue.
    /// </summary>
    public int QueuedExecutions { get; set; }

    /// <summary>
>>>>>>> c347f0b (Restore local project)
    /// Time when the worker started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// Time when status was last updated.
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
        = DateTimeOffset.Now;
}