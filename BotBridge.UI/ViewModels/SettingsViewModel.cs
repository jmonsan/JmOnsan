using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;

namespace BotBridge.UI.ViewModels;

public sealed class SettingsViewModel
    : ViewModelBase
{
    private readonly IConfigurationService
        _configurationService;

    private bool _enabled;

    private string _packagesFolder =
        string.Empty;

    private string _configFolder =
        string.Empty;

    private string _logsFolder =
        string.Empty;

    private int _processTimeoutMinutes;

    private bool _runMissedSchedules;

    public bool Enabled
    {
        get => _enabled;
        set => SetProperty(
            ref _enabled,
            value);
    }

    public string PackagesFolder
    {
        get => _packagesFolder;
        set => SetProperty(
            ref _packagesFolder,
            value);
    }

    public string ConfigFolder
    {
        get => _configFolder;
        set => SetProperty(
            ref _configFolder,
            value);
    }

    public string LogsFolder
    {
        get => _logsFolder;
        set => SetProperty(
            ref _logsFolder,
            value);
    }

    public int ProcessTimeoutMinutes
    {
        get => _processTimeoutMinutes;
        set => SetProperty(
            ref _processTimeoutMinutes,
            value);
    }

    public bool RunMissedSchedules
    {
        get => _runMissedSchedules;
        set => SetProperty(
            ref _runMissedSchedules,
            value);
    }

    public RelayCommand SaveCommand { get; }

    public RelayCommand ReloadCommand { get; }

    public SettingsViewModel(
        IConfigurationService configurationService)
    {
        _configurationService =
            configurationService;

        SaveCommand =
            new RelayCommand(
                () => _ = SaveAsync());

        ReloadCommand =
            new RelayCommand(
                () => _ = LoadAsync());

        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        var config =
            await _configurationService
                .LoadAsync();

        Enabled =
            config.Enabled;

        PackagesFolder =
            config.PackagesFolder;

        ConfigFolder =
            config.ConfigFolder;

        LogsFolder =
            config.LogsFolder;

        ProcessTimeoutMinutes =
            config.ProcessTimeoutMinutes;

        RunMissedSchedules =
            config.RunMissedSchedules;
    }

    private async Task SaveAsync()
    {
        var config =
            _configurationService.Current;

        config.Enabled =
            Enabled;

        config.PackagesFolder =
            PackagesFolder;

        config.ConfigFolder =
            ConfigFolder;

        config.LogsFolder =
            LogsFolder;

        config.ProcessTimeoutMinutes =
            ProcessTimeoutMinutes;

        config.RunMissedSchedules =
            RunMissedSchedules;

        await _configurationService
            .SaveAsync(config);
    }
}