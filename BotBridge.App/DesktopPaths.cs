namespace BotBridge.Core;

/// <summary>
/// Fixed, non-user-configurable folder and file locations on
/// the current Windows user's Desktop. There is no Settings
/// screen and no config key that changes these — BotBridge
/// always reads and writes at these exact paths, computed from
/// the OS Desktop folder so it works for whichever account the
/// app runs under.
/// </summary>
public static class DesktopPaths
{
    /// <summary>
    /// %USERPROFILE%\Desktop\BotBridge
    /// </summary>
    public static string Root { get; } =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.DesktopDirectory),
            "BotBridge");

    /// <summary>
    /// Holds the .bat files that represent automatable
    /// processes.
    /// </summary>
    public static string PackagesFolder { get; } =
        Path.Combine(Root, "Packages");

    /// <summary>
    /// Holds the JSON files that make up the app's
    /// "memory" (settings, execution history, status).
    /// </summary>
    public static string ConfigFolder { get; } =
        Path.Combine(Root, "Config");

    /// <summary>
    /// All log files written by BotBridge.
    /// </summary>
    public static string LogsFolder { get; } =
        Path.Combine(Root, "Logs");

    /// <summary>
<<<<<<< HEAD
=======
    /// Folder where UiPath writes its execution logs (.txt).
    /// Read-only for BotBridge; shown on the Execution Logs tab.
    /// </summary>
    public static string UiPathLogsFolder { get; } =
        @"C:\Users\Jeirone Onsan\AppData\Local\UiPath\Logs";

    /// <summary>
>>>>>>> c347f0b (Restore local project)
    /// The single source of truth for app settings and
    /// registered process/schedule definitions.
    /// </summary>
    public static string AppSettingsFile { get; } =
        Path.Combine(ConfigFolder, "appsettings.json");

    /// <summary>
    /// Persisted execution history, used to survive app
    /// restarts and prevent duplicate execution.
    /// </summary>
    public static string ExecutionHistoryFile { get; } =
        Path.Combine(ConfigFolder, "execution-history.json");

    /// <summary>
    /// Persisted worker/dashboard status snapshot.
    /// </summary>
    public static string StatusFile { get; } =
        Path.Combine(ConfigFolder, "status.json");
}
