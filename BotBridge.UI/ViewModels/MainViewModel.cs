using BotBridge.UI.Commands;
using BotBridge.UI.Views;

namespace BotBridge.UI.ViewModels;

public sealed class MainViewModel : ViewModelBase
{
    private object? _currentView;

    private string _activeTab = "Dashboard";

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

    /// <summary>
    /// Drives sidebar nav highlighting only — purely presentational,
    /// does not affect which view is actually shown.
    /// </summary>
    public string ActiveTab
    {
        get => _activeTab;
        private set
        {
            if (SetProperty(ref _activeTab, value))
            {
                OnPropertyChanged(nameof(IsDashboardActive));
                OnPropertyChanged(nameof(IsProcessesActive));
                OnPropertyChanged(nameof(IsSchedulesActive));
                OnPropertyChanged(nameof(IsLogsActive));
            }
        }
    }

    public bool IsDashboardActive => ActiveTab == "Dashboard";

    public bool IsProcessesActive => ActiveTab == "Processes";

    public bool IsSchedulesActive => ActiveTab == "Schedules";

    public bool IsLogsActive => ActiveTab == "Logs";

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
            new RelayCommand(() =>
            {
                CurrentView = _dashboardView;
                ActiveTab = "Dashboard";
            });

        ShowProcessesCommand =
            new RelayCommand(() =>
            {
                CurrentView = _processesView;
                ActiveTab = "Processes";
            });

        ShowSchedulesCommand =
            new RelayCommand(() =>
            {
                CurrentView = _schedulesView;
                ActiveTab = "Schedules";
            });

        ShowLogsCommand =
            new RelayCommand(() =>
            {
                CurrentView = _logsView;
                ActiveTab = "Logs";
            });

        // Dashboard is the initial view.
        CurrentView = _dashboardView;
        ActiveTab = "Dashboard";
    }
}
