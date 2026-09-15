using BotBridge.UI.Commands;
using BotBridge.UI.Views;

namespace BotBridge.UI.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private object? _currentView;

    public object? CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public RelayCommand ShowDashboardCommand { get; }

    public RelayCommand ShowProcessesCommand { get; }

    public RelayCommand ShowSchedulesCommand { get; }

    public RelayCommand ShowLogsCommand { get; }

    public RelayCommand ShowSettingsCommand { get; }

    public MainViewModel()
    {
        ShowDashboardCommand =
            new RelayCommand(() =>
                CurrentView = new DashboardView());

        ShowProcessesCommand =
            new RelayCommand(() =>
                CurrentView = new ProcessesView());

        ShowSchedulesCommand =
            new RelayCommand(() =>
                CurrentView = new SchedulesView());

        ShowLogsCommand =
            new RelayCommand(() =>
                CurrentView = new LogsView());

        ShowSettingsCommand =
            new RelayCommand(() =>
                CurrentView = new SettingsView());

        CurrentView =
            new DashboardView();
    }
}
