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
<<<<<<< HEAD
    private readonly LogsView _logsView;
=======
    private readonly RunQueueView _runQueueView;
    private readonly LogsView _logsView;
    private readonly ExecutionLogsView _executionLogsView;
>>>>>>> c347f0b (Restore local project)

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
<<<<<<< HEAD
                OnPropertyChanged(nameof(IsLogsActive));
=======
                OnPropertyChanged(nameof(IsRunQueueActive));
                OnPropertyChanged(nameof(IsLogsActive));
                OnPropertyChanged(nameof(IsExecutionLogsActive));
>>>>>>> c347f0b (Restore local project)
            }
        }
    }

    public bool IsDashboardActive => ActiveTab == "Dashboard";

    public bool IsProcessesActive => ActiveTab == "Processes";

    public bool IsSchedulesActive => ActiveTab == "Schedules";

<<<<<<< HEAD
    public bool IsLogsActive => ActiveTab == "Logs";

=======
    public bool IsRunQueueActive => ActiveTab == "RunQueue";

    public bool IsLogsActive => ActiveTab == "Logs";

    public bool IsExecutionLogsActive => ActiveTab == "ExecutionLogs";

>>>>>>> c347f0b (Restore local project)
    public RelayCommand ShowDashboardCommand { get; }

    public RelayCommand ShowProcessesCommand { get; }

    public RelayCommand ShowSchedulesCommand { get; }

<<<<<<< HEAD
    public RelayCommand ShowLogsCommand { get; }

=======
    public RelayCommand ShowRunQueueCommand { get; }

    public RelayCommand ShowLogsCommand { get; }

    public RelayCommand ShowExecutionLogsCommand { get; }

>>>>>>> c347f0b (Restore local project)
    public MainViewModel(
        DashboardView dashboardView,
        ProcessesView processesView,
        SchedulesView schedulesView,
<<<<<<< HEAD
        LogsView logsView)
=======
        RunQueueView runQueueView,
        LogsView logsView,
        ExecutionLogsView executionLogsView)
>>>>>>> c347f0b (Restore local project)
    {
        _dashboardView = dashboardView;
        _processesView = processesView;
        _schedulesView = schedulesView;
<<<<<<< HEAD
        _logsView = logsView;
=======
        _runQueueView = runQueueView;
        _logsView = logsView;
        _executionLogsView = executionLogsView;
>>>>>>> c347f0b (Restore local project)

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

<<<<<<< HEAD
=======
        ShowRunQueueCommand =
            new RelayCommand(() =>
            {
                CurrentView = _runQueueView;
                ActiveTab = "RunQueue";
            });

>>>>>>> c347f0b (Restore local project)
        ShowLogsCommand =
            new RelayCommand(() =>
            {
                CurrentView = _logsView;
                ActiveTab = "Logs";
            });

<<<<<<< HEAD
=======
        ShowExecutionLogsCommand =
            new RelayCommand(() =>
            {
                CurrentView = _executionLogsView;
                ActiveTab = "ExecutionLogs";
            });

>>>>>>> c347f0b (Restore local project)
        // Dashboard is the initial view.
        CurrentView = _dashboardView;
        ActiveTab = "Dashboard";
    }
}
