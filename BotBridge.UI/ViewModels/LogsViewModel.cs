using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;

namespace BotBridge.UI.ViewModels;

public sealed class LogsViewModel
    : ViewModelBase
{
    private readonly ILoggerService _loggerService;

    private readonly IConfigurationService
        _configurationService;

    private LogFileItemModel? _selectedLogFile;

    public ObservableCollection<LogFileItemModel>
        LogFiles { get; } = [];

    public LogFileItemModel? SelectedLogFile
    {
        get => _selectedLogFile;
        set => SetProperty(
            ref _selectedLogFile,
            value);
    }

    public RelayCommand RefreshCommand
    {
        get;
    }

    public LogsViewModel(
        ILoggerService loggerService,
        IConfigurationService configurationService)
    {
        _loggerService = loggerService;

        _configurationService =
            configurationService;

        RefreshCommand =
            new RelayCommand(
                () => _ = LoadLogFilesAsync());

        _ = LoadLogFilesAsync();
    }

    /// <summary>
    /// Browses the Logs folder and lists each log file
    /// (one entry per file), newest first.
    /// </summary>
    private async Task LoadLogFilesAsync()
    {
        LogFiles.Clear();

        var logsFolder =
            _configurationService.Current.LogsFolder;

        if (!Directory.Exists(logsFolder))
        {
            return;
        }

        var files =
            Directory.GetFiles(
                logsFolder,
                "*.log",
                SearchOption.TopDirectoryOnly);

        foreach (var file in files
                     .Select(x => new FileInfo(x))
                     .OrderByDescending(x =>
                         x.LastWriteTime))
        {
            LogFiles.Add(
                new LogFileItemModel
                {
                    FileName = file.Name,
                    FullPath = file.FullName,
                    LastModified = file.LastWriteTime,
                    SizeInBytes = file.Length
                });
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Reads a log file's full text content, for display
    /// in the drill-in popup window.
    /// </summary>
    public async Task<string> ReadLogFileAsync(
        string fullPath)
    {
        try
        {
            return await File.ReadAllTextAsync(
                fullPath);
        }
        catch (Exception ex)
        {
            await _loggerService.ErrorAsync(ex);

            return
                $"Unable to read log file: {ex.Message}";
        }
    }
}
