using System.Collections.ObjectModel;
using BotBridge.Core.Interfaces;
using BotBridge.UI.Commands;
using BotBridge.UI.Models;

namespace BotBridge.UI.ViewModels;

/// <summary>
/// Same list-and-drill-in pattern as the App Logs tab, but backed
/// by <see cref="IExecutionLogService"/>: the UiPath .txt files at
/// DesktopPaths.UiPathLogsFolder, auto-refreshed by the view every
/// 5 seconds so new, modified and rotated files show up without
/// duplicating or losing content.
/// </summary>
public sealed class ExecutionLogsViewModel
    : ViewModelBase
{
    private readonly IExecutionLogService _executionLogService;

    private ExecutionLogFileModel? _selectedLogFile;

    private bool _folderAvailable = true;

    private string? _statusMessage;

    public ObservableCollection<ExecutionLogFileModel>
        LogFiles { get; } = [];

    public ExecutionLogFileModel? SelectedLogFile
    {
        get => _selectedLogFile;
        set => SetProperty(
            ref _selectedLogFile,
            value);
    }

    public bool FolderAvailable
    {
        get => _folderAvailable;
        private set => SetProperty(
            ref _folderAvailable,
            value);
    }

    public string? StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(
            ref _statusMessage,
            value);
    }

    public RelayCommand RefreshCommand { get; }

    public ExecutionLogsViewModel(
        IExecutionLogService executionLogService)
    {
        _executionLogService = executionLogService;

        RefreshCommand =
            new RelayCommand(
                () => _ = RefreshAsync());

        _ = RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        var snapshot =
            await _executionLogService.RefreshAsync();

        FolderAvailable = snapshot.FolderAvailable;

        StatusMessage =
            snapshot.FolderAvailable
                ? null
                : snapshot.Error
                  ?? $"UiPath log folder not available: {_executionLogService.FolderPath}";

        var incoming =
            snapshot.Files
                .ToDictionary(x => x.FullPath);

        for (var i = LogFiles.Count - 1; i >= 0; i--)
        {
            if (!incoming.ContainsKey(
                    LogFiles[i].FullPath))
            {
                LogFiles.RemoveAt(i);
            }
        }

        var existingPaths =
            new HashSet<string>(
                LogFiles.Select(x => x.FullPath));

        foreach (var file in snapshot.Files)
        {
            if (existingPaths.Contains(file.FullPath))
            {
                var existing =
                    LogFiles.First(x =>
                        x.FullPath == file.FullPath);

                existing.LastModified = file.LastModified;
                existing.SizeInBytes = file.SizeInBytes;
            }
            else
            {
                LogFiles.Add(
                    new ExecutionLogFileModel
                    {
                        FileName = file.FileName,
                        FullPath = file.FullPath,
                        LastModified = file.LastModified,
                        SizeInBytes = file.SizeInBytes
                    });
            }
        }
    }

    /// <summary>
    /// Reads a log file's current tailed content, for display in
    /// the drill-in popup window.
    /// </summary>
    public async Task<string> ReadLogFileAsync(
        string fullPath)
    {
        return await _executionLogService.GetContentAsync(
            fullPath);
    }
}
