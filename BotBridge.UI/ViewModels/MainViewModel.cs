using BotBridge.UI.Commands;
using BotBridge.UI.Views;

namespace BotBridge.UI.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private object? _currentView;

    private readonly DashboardView _dashboardView;
    private readonly ProcessesView _processesView;
    private readonly SchedulesView _schedulesView;
    private readonly LogsView _logsView;

    public object? CurrentView
    {
        get => _currentView;
        private set => SetProperty(
            ref _currentView,
            value);
    }

    public RelayCommand ShowDashboardCommand { get; }

    public RelayCommand ShowProcessesCommand { get; }

    public RelayCommand ShowSchedulesCommand { get; }

    public RelayCommand ShowLogsCommand { get; }

    public MainViewModel(
        DashboardView dashboardView,
        ProcessesView processesView,
        SchedulesView schedulesView,
        LogsView logsView)
    {
        _dashboardView = dashboardView;
        _processesView = processesView;
        _schedulesView = schedulesView;
        _logsView = logsView;

        ShowDashboardCommand =
            new RelayCommand(
                () => CurrentView =
                    _dashboardView);

        ShowProcessesCommand =
            new RelayCommand(
                () => CurrentView =
                    _processesView);

        ShowSchedulesCommand =
            new RelayCommand(
                () => CurrentView =
                    _schedulesView);

        ShowLogsCommand =
            new RelayCommand(
                () => CurrentView =
                    _logsView);

        // Dashboard is the initial view.
        CurrentView = _dashboardView;
    }
}