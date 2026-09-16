using System.Windows.Threading;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;

namespace BotBridge.UI.ViewModels;

/// <summary>
/// Read-only view of the backend worker's status. No manual
/// Start/Stop controls — the worker runs continuously for the
/// lifetime of the app (see App.xaml.cs). This view control
/// re-checks every 30 seconds while it's open, in addition to
/// updating immediately whenever the backend status changes.
/// </summary>
public sealed class DashboardViewModel
    : ViewModelBase
{
    private static readonly TimeSpan RefreshInterval =
        TimeSpan.FromSeconds(30);

    private readonly IStatusService _statusService;

    private readonly DispatcherTimer _refreshTimer;

    private BackendStatus _snapshot;

    private DateTime _currentDateTime = DateTime.Now;

    public string Status =>
        _snapshot.State.ToString();

    public string CurrentTask =>
        _snapshot.CurrentProcess
        ?? "Waiting...";

    public string RunningCount =>
        // BotBridge executes one process at a time, so
        // "currently running" is 1 while Executing, 0 otherwise.
        _snapshot.State == WorkerState.Executing
            ? "1"
            : "0";

    public string CurrentDateTime =>
        _currentDateTime.ToString(
            "yyyy-MM-dd HH:mm:ss");

    public string NextSchedule =>
        _snapshot.NextScheduledRun?
            .ToString("yyyy-MM-dd HH:mm:ss")
        ?? "N/A";

    public string LastExecution =>
        _snapshot.LastExecutionTime?
            .ToString("yyyy-MM-dd HH:mm:ss")
        ?? "N/A";

    public string LastResult =>
        _snapshot.LastResult?.Message
        ?? "N/A";

    public DashboardViewModel(
        IStatusService statusService)
    {
        _statusService =
            statusService;

        _snapshot =
            _statusService.GetSnapshot();

        _statusService.StatusChanged +=
            OnStatusChanged;

        _refreshTimer =
            new DispatcherTimer
            {
                Interval = RefreshInterval
            };

        _refreshTimer.Tick +=
            (_, _) => Poll();

        _refreshTimer.Start();
    }

    /// <summary>
    /// The 30-second polling tick: re-pulls the latest status
    /// snapshot and refreshes the current date/time.
    /// </summary>
    private void Poll()
    {
        _snapshot =
            _statusService.GetSnapshot();

        _currentDateTime =
            DateTime.Now;

        Refresh();
    }

    private void OnStatusChanged(
        object? sender,
        BackendStatus snapshot)
    {
        var dispatcher =
            System.Windows.Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            ApplySnapshot(snapshot);
        }
        else
        {
            dispatcher.BeginInvoke(
                () => ApplySnapshot(snapshot));
        }
    }

    private void ApplySnapshot(
        BackendStatus snapshot)
    {
        _snapshot = snapshot;

        Refresh();
    }

    public void Refresh()
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(CurrentTask));
        OnPropertyChanged(nameof(RunningCount));
        OnPropertyChanged(nameof(CurrentDateTime));
        OnPropertyChanged(nameof(NextSchedule));
        OnPropertyChanged(nameof(LastExecution));
        OnPropertyChanged(nameof(LastResult));
    }
}
