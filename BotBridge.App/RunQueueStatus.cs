namespace BotBridge.Core.Models;

/// <summary>
/// State of an entry that is tracked by the Run Queue.
/// </summary>
public enum RunQueueStatus
{
    /// <summary>
    /// Waiting for a free execution slot.
    /// </summary>
    Queued = 0,

    /// <summary>
    /// Currently executing.
    /// </summary>
    Running = 1
}
