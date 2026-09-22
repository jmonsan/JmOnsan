using BotBridge.UI.ViewModels;

namespace BotBridge.UI.Models;

/// <summary>
/// A Run Queue row. Extends ViewModelBase (rather than being a
/// plain record like the other list-item models) because its
/// status can change in place — Queued to Running — while the
/// row itself stays on screen.
/// </summary>
public sealed class RunQueueItemModel
    : ViewModelBase
{
    private string _statusText = string.Empty;

    public Guid Id { get; set; }

    public string ProcessName { get; set; } = string.Empty;

    /// <summary>
    /// "Running" or "Queued" — matches WorkerState-style text so
    /// the existing StatusToBrushConverter colors it correctly.
    /// </summary>
    public string StatusText
    {
        get => _statusText;
        set => SetProperty(
            ref _statusText,
            value);
    }

    public bool IsManual { get; set; }

    public string Source =>
        IsManual ? "Manual" : "Scheduled";
}
