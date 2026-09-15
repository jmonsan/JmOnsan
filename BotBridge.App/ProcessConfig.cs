namespace BotBridge.Core.Models;

public sealed class ProcessConfig
{
    /// <summary>
    /// Display name shown to users.
    /// Example: AWIR
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Enables/disables this process.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// BAT file name inside the Packages folder.
    /// Example: AWIR.bat
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Schedule assigned to this process.
    /// </summary>
    public ScheduleDefinition Schedule { get; set; } = new();

    /// <summary>
    /// Maximum runtime before timeout.
    /// Optional override of global timeout.
    /// </summary>
    public int? TimeoutMinutes { get; set; }

    /// <summary>
    /// Number of retry attempts after failure.
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// Additional notes.
    /// </summary>
    public string Description { get; set; } = string.Empty;
}