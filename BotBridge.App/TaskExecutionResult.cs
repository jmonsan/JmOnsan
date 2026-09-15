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
}