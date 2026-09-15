using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;

namespace BotBridge.UI.ViewModels;

public sealed class DashboardViewModel
    : ViewModelBase
{
    private readonly IStatusService _statusService;

    public string Status =>
        _statusService.Current.State.ToString();

    public string CurrentTask =>
        _statusService.Current.CurrentProcess
        ?? "Waiting...";

    public string NextSchedule =>
        _statusService.Current.NextScheduledRun?
            .ToString("yyyy-MM-dd HH:mm:ss")
        ?? "N/A";

    public string LastExecution =>
        _statusService.Current.LastExecutionTime?
            .ToString("yyyy-MM-dd HH:mm:ss")
        ?? "N/A";

    public string LastResult =>
        _statusService.Current.LastResult?.Message
        ?? "N/A";

    public RelayCommand StartCommand { get; }

    public RelayCommand StopCommand { get; }

    public DashboardViewModel(
        IStatusService statusService)
    {
        _statusService =
            statusService;

        _statusService.StatusChanged +=
            OnStatusChanged;

        StartCommand =
            new RelayCommand(() =>
            {
                // Future:
                // IApplicationControlService.StartAsync()
            });

        StopCommand =
            new RelayCommand(() =>
            {
                // Future:
                // IApplicationControlService.StopAsync()
            });
    }

    private void OnStatusChanged(
        object? sender,
        Core.Models.BackendStatus e)
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(CurrentTask));
        OnPropertyChanged(nameof(NextSchedule));
        OnPropertyChanged(nameof(LastExecution));
        OnPropertyChanged(nameof(LastResult));
    }
}