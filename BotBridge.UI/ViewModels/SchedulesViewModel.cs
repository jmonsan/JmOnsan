using System.Collections.ObjectModel;
using BotBridge.Core.Interfaces;
using BotBridge.Core.Models;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;

namespace BotBridge.UI.ViewModels;

public sealed class SchedulesViewModel
    : ViewModelBase
{
    private readonly IConfigurationService
        _configurationService;

    private readonly IProcessService
        _processService;

    private ScheduleItemModel?
        _selectedSchedule;

    private string?
        _selectedProcessName;

    private ScheduleType
        _selectedScheduleType = ScheduleType.Once;

    private string
        _startDateTimeText =
            DateTime.Now.ToString(
                "yyyy-MM-dd HH:mm:ss");

    private DayOfWeek
        _selectedDayOfWeek = DateTime.Now.DayOfWeek;

    private bool _scheduleEnabled = true;

    private bool _runMissedExecution = true;

    private bool _preventDuplicateExecution = true;

    public ObservableCollection<ScheduleItemModel>
        Schedules { get; } = [];

    public ObservableCollection<string>
        AvailableProcesses { get; } = [];

    public IReadOnlyList<ScheduleType> ScheduleTypes { get; }
        = Enum.GetValues<ScheduleType>();

    public IReadOnlyList<DayOfWeek> DaysOfWeek { get; }
        = Enum.GetValues<DayOfWeek>();

    public ScheduleItemModel?
        SelectedSchedule
    {
        get => _selectedSchedule;
        set
        {
            if (SetProperty(
                    ref _selectedSchedule,
                    value) &&
                value is not null)
            {
                SelectedProcessName =
                    value.ProcessName;
            }
        }
    }

    public string? SelectedProcessName
    {
        get => _selectedProcessName;
        set
        {
            if (SetProperty(
                    ref _selectedProcessName,
                    value))
            {
                PopulateEditorFromExistingSchedule();
            }
        }
    }

    public ScheduleType SelectedScheduleType
    {
        get => _selectedScheduleType;
        set => SetProperty(
            ref _selectedScheduleType,
            value);
    }

    public string StartDateTimeText
    {
        get => _startDateTimeText;
        set => SetProperty(
            ref _startDateTimeText,
            value);
    }

    public DayOfWeek SelectedDayOfWeek
    {
        get => _selectedDayOfWeek;
        set => SetProperty(
            ref _selectedDayOfWeek,
            value);
    }

    public bool ScheduleEnabled
    {
        get => _scheduleEnabled;
        set => SetProperty(
            ref _scheduleEnabled,
            value);
    }

    public bool RunMissedExecution
    {
        get => _runMissedExecution;
        set => SetProperty(
            ref _runMissedExecution,
            value);
    }

    public bool PreventDuplicateExecution
    {
        get => _preventDuplicateExecution;
        set => SetProperty(
            ref _preventDuplicateExecution,
            value);
    }

    public RelayCommand RefreshCommand
    {
        get;
    }

    public RelayCommand SaveCommand
    {
        get;
    }

    public SchedulesViewModel(
        IConfigurationService configurationService,
        IProcessService processService)
    {
        _configurationService =
            configurationService;

        _processService =
            processService;

        RefreshCommand =
            new RelayCommand(
                () => _ = LoadAsync());

        SaveCommand =
            new RelayCommand(
                () => _ = SaveScheduleAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        await LoadProcessesAsync();

        await LoadSchedulesAsync();
    }

    private async Task LoadProcessesAsync()
    {
        AvailableProcesses.Clear();

        var processes =
            await _processService
                .GetProcessesAsync();

        foreach (var process in processes)
        {
            AvailableProcesses.Add(
                process.Name);
        }

        SelectedProcessName ??=
            AvailableProcesses.FirstOrDefault();
    }

    private async Task LoadSchedulesAsync()
    {
        Schedules.Clear();

        var config =
            await _configurationService.LoadAsync();

        foreach (var process in config.Processes)
        {
            Schedules.Add(
                new ScheduleItemModel
                {
                    ProcessName =
                        process.Name,

                    ScheduleType =
                        process.Schedule.Type
                            .ToString(),

                    StartDateTime =
                        process.Schedule
                            .StartDateTime,

                    Enabled =
                        process.Schedule
                            .Enabled
                });
        }
    }

    /// <summary>
    /// When the process dropdown selection changes,
    /// loads that process's existing schedule into the
    /// editor fields (update flow), or resets the
    /// editor to sensible defaults for a brand-new
    /// schedule (create flow).
    /// </summary>
    private void PopulateEditorFromExistingSchedule()
    {
        if (SelectedProcessName is null)
        {
            return;
        }

        var existing =
            _configurationService.Current.Processes
                .FirstOrDefault(x =>
                    x.Name == SelectedProcessName);

        if (existing is null)
        {
            SelectedScheduleType = ScheduleType.Once;

            StartDateTimeText =
                DateTime.Now.ToString(
                    "yyyy-MM-dd HH:mm:ss");

            SelectedDayOfWeek =
                DateTime.Now.DayOfWeek;

            ScheduleEnabled = true;
            RunMissedExecution = true;
            PreventDuplicateExecution = true;

            return;
        }

        var schedule = existing.Schedule;

        SelectedScheduleType = schedule.Type;

        StartDateTimeText =
            schedule.StartDateTime.ToString(
                "yyyy-MM-dd HH:mm:ss");

        SelectedDayOfWeek =
            schedule.DayOfWeek ??
            schedule.StartDateTime.DayOfWeek;

        ScheduleEnabled = schedule.Enabled;
        RunMissedExecution = schedule.RunMissedExecution;

        PreventDuplicateExecution =
            schedule.PreventDuplicateExecution;
    }

    private async Task SaveScheduleAsync()
    {
        if (string.IsNullOrWhiteSpace(
                SelectedProcessName))
        {
            return;
        }

        if (!DateTimeOffset.TryParse(
                StartDateTimeText,
                out var startDateTime))
        {
            return;
        }

        var config =
            _configurationService.Current;

        var existing =
            config.Processes.FirstOrDefault(x =>
                x.Name == SelectedProcessName);

        var scheduleDefinition = new ScheduleDefinition
        {
            Type = SelectedScheduleType,
            StartDateTime = startDateTime,
            Enabled = ScheduleEnabled,
            RunMissedExecution = RunMissedExecution,

            PreventDuplicateExecution =
                PreventDuplicateExecution,

            DayOfWeek =
                SelectedScheduleType ==
                    ScheduleType.Weekly
                    ? SelectedDayOfWeek
                    : null
        };

        if (existing is not null)
        {
            existing.Schedule = scheduleDefinition;
        }
        else
        {
            config.Processes.Add(
                new ProcessConfig
                {
                    Name = SelectedProcessName,
                    Enabled = true,

                    FileName =
                        $"{SelectedProcessName}.bat",

                    Schedule = scheduleDefinition
                });
        }

        await _configurationService.SaveAsync(
            config);

        await LoadSchedulesAsync();
    }
}
