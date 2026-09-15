using System.Collections.ObjectModel;
using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;

namespace BotBridge.UI.ViewModels;

public sealed class LogsViewModel
    : ViewModelBase
{
    private readonly ILoggerService _loggerService;

    public ObservableCollection<LogItemModel>
        Logs { get; } = [];

    public RelayCommand RefreshCommand
    {
        get;
    }

    public LogsViewModel(
        ILoggerService loggerService)
    {
        _loggerService = loggerService;

        RefreshCommand =
            new RelayCommand(
                () => _ = LoadLogsAsync());

        _ = LoadLogsAsync();
    }

    private async Task LoadLogsAsync()
    {
        Logs.Clear();

        var entries =
            await _loggerService
                .GetRecentLogsAsync(200);

        foreach (var entry in entries
                     .OrderByDescending(x => x.Timestamp))
        {
            Logs.Add(
                new LogItemModel
                {
                    Timestamp = entry.Timestamp,
                    Level = entry.Level,
                    Message = entry.Message
                });
        }
    }
}