using System.Collections.ObjectModel;
using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;

namespace BotBridge.UI.ViewModels;

public sealed class SchedulesViewModel
    : ViewModelBase
{
    private readonly IConfigurationService
        _configurationService;

    private ScheduleItemModel?
        _selectedSchedule;

    public ObservableCollection<ScheduleItemModel>
        Schedules { get; } = [];

    public ScheduleItemModel?
        SelectedSchedule
    {
        get => _selectedSchedule;
        set => SetProperty(
            ref _selectedSchedule,
            value);
    }

    public RelayCommand RefreshCommand
    {
        get;
    }

    public SchedulesViewModel(
        IConfigurationService configurationService)
    {
        _configurationService =
            configurationService;

        RefreshCommand =
            new RelayCommand(
                () => _ = LoadSchedulesAsync());

        _ = LoadSchedulesAsync();
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
}