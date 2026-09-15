using System.Windows;
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

    private BackendStatus _snapshot;

    public string Status =>
        _snapshot.State.ToString();

    public string CurrentTask =>
        _snapshot.CurrentProcess
        ?? "Waiting...";

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

        _snapshot =
            _statusService.GetSnapshot();

        StartCommand =
            new RelayCommand(
                () => _ =
                    StartAsync());

        StopCommand =
            new RelayCommand(
                () =>
                    _ =
                        StopAsync());

        _statusService.StatusChanged +=
            OnStatusChanged;
    }

    private async Task StartAsync()
    {
        try
        {
            await _applicationControlService
                .StartAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Unable to start BotBridge",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private async Task StopAsync()
    {
        try
        {
            await _applicationControlService
                .StopAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                ex.Message,
                "Unable to stop BotBridge",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
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
        OnPropertyChanged(nameof(NextSchedule));
        OnPropertyChanged(nameof(LastExecution));
        OnPropertyChanged(nameof(LastResult));
    }
}