namespace BotBridge.Core.Models;

public sealed class AppConfig
{
    /// <summary>
    /// Global application switch.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Root folder containing BAT processes.
    /// </summary>
    public string PackagesFolder { get; set; } = @"C:\BotBridge\Packages";

    /// <summary>
    /// Root folder containing configuration files.
    /// </summary>
    public string ConfigFolder { get; set; } = @"C:\BotBridge\Config";

    /// <summary>
    /// Log output folder.
    /// </summary>
    public string LogsFolder { get; set; } = @"C:\BotBridge\Logs";

    /// <summary>
    /// Maximum number of concurrent process executions.
    /// </summary>
    public int MaxConcurrentTasks { get; set; } = 1;

    /// <summary>
    /// Timeout applied to BAT execution.
    /// </summary>
    public int ProcessTimeoutMinutes { get; set; } = 60;

    /// <summary>
    /// Execute missed schedules after restart/resume.
    /// </summary>
    public bool RunMissedSchedules { get; set; } = true;

    /// <summary>
    /// Registered process schedules.
    /// </summary>
    public List<ProcessConfig> Processes { get; set; } = new();

    /// <summary>
    /// Logging settings.
    /// </summary>
    public LoggingConfig Logging { get; set; } = new();
}