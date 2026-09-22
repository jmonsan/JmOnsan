<<<<<<< HEAD
=======
using System.Text.Json.Serialization;

>>>>>>> c347f0b (Restore local project)
namespace BotBridge.Core.Models;

public sealed class TaskExecutionResult
{
    public Guid ExecutionId { get; set; }

    public string ProcessName { get; set; }
        = string.Empty;

    public bool Success { get; set; }

    public int? ProcessId { get; set; }

    public DateTimeOffset StartedAt { get; set; }

    public DateTimeOffset FinishedAt { get; set; }

    public TimeSpan Duration =>
        FinishedAt - StartedAt;

    public string Message { get; set; }
        = string.Empty;

    public string? Error { get; set; }
<<<<<<< HEAD
=======

    /// <summary>
    /// True when the process could not start because another
    /// UiPath foreground process was already running. The worker
    /// puts such an execution back into the Run Queue instead of
    /// treating it as a failure. Not persisted.
    /// </summary>
    [JsonIgnore]
    public bool IsConflict { get; set; }
>>>>>>> c347f0b (Restore local project)
}