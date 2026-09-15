using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;
using BotBridge.UI.Commands;

namespace BotBridge.UI.ViewModels;

public sealed class DashboardViewModel
    : ViewModelBase
{
    private readonly IStatusService _statusService;

    private readonly IApplicationControlService
        _applicationControlService;

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
        IStatusService statusService,
        IApplicationControlService applicationControlService)
    {
        _statusService =
            statusService;

        _applicationControlService =
            applicationControlService;

        StartCommand =
            new RelayCommand(
                () => _ =
                    _applicationControlService.StartAsync());

        StopCommand =
            new RelayCommand(
                () => _ =
                    _applicationControlService.StopAsync());

        _statusService.StatusChanged +=
            OnStatusChanged;
    }

    private void OnStatusChanged(
        object? sender,
        BackendStatus e)
    {
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(CurrentTask));
        OnPropertyChanged(nameof(NextSchedule));
        OnPropertyChanged(nameof(LastExecution));
        OnPropertyChanged(nameof(LastResult));
    }
}