using BotBridge.UI.ViewModels;

namespace BotBridge.UI.Models;

/// <summary>
/// Extends ViewModelBase (unlike the static App Logs row model)
/// because this tab auto-refreshes every 5 seconds and an existing
/// file's size/last-modified need to update in place.
/// </summary>
public sealed class ExecutionLogFileModel
    : ViewModelBase
{
    private DateTimeOffset _lastModified;

    private long _sizeInBytes;

    public string FileName { get; set; } = string.Empty;

    public string FullPath { get; set; } = string.Empty;

    public DateTimeOffset LastModified
    {
        get => _lastModified;
        set => SetProperty(
            ref _lastModified,
            value);
    }

    public long SizeInBytes
    {
        get => _sizeInBytes;
        set => SetProperty(
            ref _sizeInBytes,
            value);
    }
}
