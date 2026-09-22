using BotBridge.Core.Models;

namespace BotBridge.Core.Interfaces;

/// <summary>
/// Thread-safe queue of pending process executions.
///
/// Every due execution passes through this queue. Executions that
/// can start right away run immediately and never appear in the
/// Run Queue tab. Executions that collide with a running process
/// (a scheduling conflict) wait here until a slot is free — they
/// are never skipped and the running process is never killed.
/// </summary>
public interface IRunQueueService
{
    /// <summary>
    /// Raised whenever the set of entries shown on the Run Queue
    /// tab (or their status) changes. May be raised from any thread.
    /// </summary>
    event EventHandler? Changed;

    /// <summary>
    /// Number of entries currently executing.
    /// </summary>
    int RunningCount { get; }

    /// <summary>
    /// Number of entries waiting for a free slot.
    /// </summary>
    int QueuedCount { get; }

    /// <summary>
    /// Entries that belong on the Run Queue tab, oldest first.
    /// </summary>
    IReadOnlyList<RunQueueItem> GetSnapshot();

    /// <summary>
    /// Names of the processes that are executing right now.
    /// </summary>
    IReadOnlyList<string> GetRunningProcessNames();

    /// <summary>
    /// Adds an execution to the queue. The entry becomes visible
    /// on the Run Queue tab only if it cannot start immediately.
    /// </summary>
    RunQueueItem Enqueue(
        ScheduledProcess scheduledProcess,
        bool isManual,
        int maxConcurrent);

    /// <summary>
    /// Marks the next startable entry as running and returns it,
    /// or returns null when nothing can start right now.
    /// </summary>
    RunQueueItem? TryStartNext(
        int maxConcurrent,
        DateTimeOffset now);

    /// <summary>
    /// Puts a running entry back into the queue (for example when
    /// another UiPath foreground process blocked it). The entry
    /// becomes visible and is retried after <paramref name="notBefore"/>.
    /// </summary>
    void Requeue(
        Guid id,
        DateTimeOffset notBefore,
        string reason);

    /// <summary>
    /// Removes a finished entry from the queue.
    /// </summary>
    void Complete(Guid id);

    /// <summary>
    /// Removes every entry.
    /// </summary>
    void Clear();

    /// <summary>
    /// Appends a line to the entry's log.
    /// </summary>
    void AppendLog(
        Guid id,
        string line);

    /// <summary>
    /// Returns the entry's log, or null when the entry is gone.
    /// </summary>
    string? GetLogText(Guid id);
}
