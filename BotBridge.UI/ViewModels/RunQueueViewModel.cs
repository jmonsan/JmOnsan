using System.Collections.ObjectModel;
using System.Windows.Threading;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;

namespace BotBridge.UI.ViewModels;

/// <summary>
/// Rows added/removed dynamically as processes conflict, wait,
/// and start. A due process is never skipped or killed — it sits
/// here until a slot frees up. Refreshes on every
/// <see cref="IRunQueueService.Changed"/> event and on a short
/// timer as a safety net while the tab is open.
/// </summary>
public sealed class RunQueueViewModel
    : ViewModelBase
{
    private static readonly TimeSpan RefreshInterval =
        TimeSpan.FromSeconds(2);

    private readonly IRunQueueService _runQueue;

    private readonly DispatcherTimer _refreshTimer;

    public ObservableCollection<RunQueueItemModel>
        Items { get; } = [];

    public RelayCommand RefreshCommand { get; }

    public RunQueueViewModel(
        IRunQueueService runQueue)
    {
        _runQueue = runQueue;

        RefreshCommand =
            new RelayCommand(Refresh);

        _runQueue.Changed +=
            OnQueueChanged;

        _refreshTimer =
            new DispatcherTimer
            {
                Interval = RefreshInterval
            };

        _refreshTimer.Tick +=
            (_, _) => Refresh();

        Refresh();
    }

    public void Start()
    {
        _refreshTimer.Start();
    }

    public void Stop()
    {
        _refreshTimer.Stop();
    }

    /// <summary>
    /// Returns the current log text for a row, for the eye-icon
    /// popup viewer.
    /// </summary>
    public string GetLogText(Guid id)
    {
        return _runQueue.GetLogText(id)
            ?? "This entry has already finished.";
    }

    private void OnQueueChanged(
        object? sender,
        EventArgs e)
    {
        var dispatcher =
            System.Windows.Application.Current?.Dispatcher;

        if (dispatcher is null ||
            dispatcher.CheckAccess())
        {
            Refresh();
        }
        else
        {
            dispatcher.BeginInvoke(Refresh);
        }
    }

    private void Refresh()
    {
        var snapshot =
            _runQueue.GetSnapshot();

        // Rebuild in place: dynamically add/remove rows without
        // flashing the whole grid on every tick.
        var currentIds =
            new HashSet<Guid>(
                Items.Select(x => x.Id));

        var snapshotIds =
            new HashSet<Guid>(
                snapshot.Select(x => x.Id));

        for (var i = Items.Count - 1; i >= 0; i--)
        {
            if (!snapshotIds.Contains(Items[i].Id))
            {
                Items.RemoveAt(i);
            }
        }

        foreach (var entry in snapshot)
        {
            var statusText =
                entry.Status == RunQueueStatus.Running
                    ? "Running"
                    : "Queued";

            if (currentIds.Contains(entry.Id))
            {
                var existing =
                    Items.First(x => x.Id == entry.Id);

                existing.StatusText = statusText;
            }
            else
            {
                Items.Add(
                    new RunQueueItemModel
                    {
                        Id = entry.Id,
                        ProcessName = entry.ProcessName,
                        StatusText = statusText,
                        IsManual = entry.IsManual
                    });
            }
        }
    }
}
